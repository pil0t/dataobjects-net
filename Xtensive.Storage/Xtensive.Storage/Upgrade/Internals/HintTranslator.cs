// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.27

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Upgrade
{
  internal sealed class HintTranslator
  {
    private readonly Dictionary<StoredTypeInfo, TypeInfo> forwardTypeMapping
      = new Dictionary<StoredTypeInfo, TypeInfo>();
    private readonly Dictionary<TypeInfo, StoredTypeInfo> backwardTypeMapping
      = new Dictionary<TypeInfo, StoredTypeInfo>();
    private readonly Dictionary<StoredFieldInfo, FieldInfo> fieldMapping
      = new Dictionary<StoredFieldInfo, FieldInfo>();
    private readonly HashSet<StoredAssociationInfo> touchedAssociations
      = new HashSet<StoredAssociationInfo>();

    private readonly StoredDomainModel oldModel;
    private readonly DomainModel newModel;
    
    public IEnumerable<Hint> Translate(IEnumerable<UpgradeHint> hints)
    {
      var renameTypeHints = hints.OfType<RenameTypeHint>().ToArray();
      var renameFieldHints = hints.OfType<RenameFieldHint>().ToArray();
      var copyFieldHints = hints.OfType<CopyFieldHint>().ToArray();

      ValidateRenameTypeHints(renameTypeHints);
      ValidateRenameFieldHints(renameFieldHints);

      renameTypeHints.Apply(ProcessRenameTypeHint);
      renameFieldHints.Apply(ProcessRenameFieldHint);
      touchedAssociations.Apply(ProcessAssociation);

      return GenerateRenameTableHints()
        .Concat(GenerateRenameColumnHints())
        .Concat(copyFieldHints.SelectMany(hint => GenerateCopyColumnHints(hint)))
        .ToArray();
    }

    private void ValidateRenameTypeHints(IEnumerable<RenameTypeHint> hints)
    {
      var sourceTypes = new Dictionary<string, RenameTypeHint>();
      var targetTypes = new Dictionary<Type, RenameTypeHint>();
      foreach (var hint in hints) {
        RenameTypeHint evilHint;
        if (sourceTypes.TryGetValue(hint.OldType, out evilHint))
          throw HintConflict(hint, evilHint);
        if (targetTypes.TryGetValue(hint.NewType, out evilHint))
          throw HintConflict(hint, evilHint);
        sourceTypes.Add(hint.OldType, hint);
        targetTypes.Add(hint.NewType, hint);
      }
    }

    private void ValidateRenameFieldHints(IEnumerable<RenameFieldHint> hints)
    {
      var sourceFields = new Dictionary<Pair<Type, string>, RenameFieldHint>();
      var targetFields = new Dictionary<Pair<Type, string>, RenameFieldHint>();
      foreach (var hint in hints) {
        RenameFieldHint evilHint;
        var sourceField = new Pair<Type, string>(hint.TargetType, hint.OldFieldName);
        var targetField = new Pair<Type, string>(hint.TargetType, hint.NewFieldName);
        if (sourceFields.TryGetValue(sourceField, out evilHint))
          throw HintConflict(hint, evilHint);
        if (targetFields.TryGetValue(targetField, out evilHint))
          throw HintConflict(hint, evilHint);
        sourceFields.Add(sourceField, hint);
        targetFields.Add(targetField, hint);
      }
    }

    private void ProcessRenameTypeHint(RenameTypeHint hint)
    {
      var oldType = ResolveOldType(hint.OldType);
      var newType = newModel.Types[hint.NewType];
      RemapType(oldType, newType);
    }

    private void ProcessRenameFieldHint(RenameFieldHint hint)
    {
      var newType = newModel.Types[hint.TargetType];
      var newField = newType.Fields[hint.NewFieldName];
      var oldType = ResolveOldType(newType);
      var oldField = ResolveOldField(oldType, hint.OldFieldName, false);
      RemapField(oldField, newField);
    }
    
    private void ProcessAssociation(StoredAssociationInfo oldAssociation)
    {
      if (oldAssociation.ConnectorType==null)
        return;

      // ignoring remap from paired association
      if (forwardTypeMapping.ContainsKey(oldAssociation.ConnectorType))
        return;

      var newReferencingField = ResolveNewField(oldAssociation.ReferencingField);
      var newAssociation = newReferencingField.Association;

      RemapType(oldAssociation.ConnectorType, newAssociation.UnderlyingType);

      var oldMaster = ResolveOldField(oldAssociation.ConnectorType, WellKnown.MasterField, true);
      var newMaster = ResolveNewField(newAssociation.UnderlyingType, WellKnown.MasterField, true);
      var oldSlave = ResolveOldField(oldAssociation.ConnectorType, WellKnown.SlaveField, true);
      var newSlave = ResolveNewField(newAssociation.UnderlyingType, WellKnown.SlaveField, true);

      RemapField(oldMaster, newMaster);
      RemapField(oldSlave, newSlave);
    }

    private void RemapField(StoredFieldInfo oldField, FieldInfo newField)
    {
      fieldMapping[oldField] = newField;
      var type = oldField.DeclaringType;
      
      if (oldField.Fields.Length>1)
        throw new NotImplementedException();
      if (oldField.Fields.Length==1)
        RemapField(oldField.Fields[0], newField.Fields[0]);

      // primary key -> rename all referencers
      if (type.IsEntity && oldField.IsPrimaryKey)
        foreach (var association in FindEntityUsages(type)) {
          var oldReferencingField = association.ReferencingField;
          var newReferencingField = ResolveNewField(oldReferencingField);
          RemapField(oldReferencingField, newReferencingField);
          touchedAssociations.Add(association);
        }

      // renaming referencing field -> (posibly) rename corresponding association
      var affectedAssociation = oldModel.Associations
        .SingleOrDefault(association => association.ReferencingField==oldField);
      if (affectedAssociation != null)
        touchedAssociations.Add(affectedAssociation);

      // renaming structure field -> rename all usages
      if (type.IsStructure)
        FindStructureUsages(type).Apply(usage => RemapField(usage, ResolveNewField(usage)));
    }

    private void RemapType(StoredTypeInfo oldType, TypeInfo newType)
    {
      if (!oldType.IsEntity)
        return;

      forwardTypeMapping[oldType] = newType;
      backwardTypeMapping[newType] = oldType;

      FindEntityUsages(oldType).Apply(association => touchedAssociations.Add(association));
    }

    private IEnumerable<StoredFieldInfo> FindStructureUsages(StoredTypeInfo type)
    {
      return oldModel.Types
        .SelectMany(t => t.Fields.Where(f => f.IsStructure && f.ValueType==type.UnderlyingType));
    }

    private IEnumerable<StoredAssociationInfo> FindEntityUsages(StoredTypeInfo type)
    {
      return oldModel.Associations
        .Where(a => a.ReferencedType==type);
    }

    #region Resolve{New,Old}{Type,Field}

    private StoredTypeInfo ResolveOldType(string name)
    {
      var type = oldModel.Types.SingleOrDefault(t => t.UnderlyingType==name);
      if (type == null)
        throw TypeIsNotFound(name);
      return type;
    }
    
    private StoredTypeInfo ResolveOldType(TypeInfo type)
    {
      StoredTypeInfo result;
      return backwardTypeMapping.TryGetValue(type, out result)
        ? result
        : ResolveOldType(type.UnderlyingType.GetFullName());
    }

    private TypeInfo ResolveNewType(StoredTypeInfo type)
    {
      TypeInfo result;
      return forwardTypeMapping.TryGetValue(type, out result)
        ? result
        : ResolveNewType(type.UnderlyingType);
    }

    private TypeInfo ResolveNewType(string name)
    {
      var result = newModel.Types
        .SingleOrDefault(t => t.UnderlyingType.GetFullName()==name);
      if (result == null)
        throw TypeIsNotFound(name);
      return result;
    }
    
    private StoredFieldInfo ResolveOldField(StoredTypeInfo type, string name, bool includeInheritedFields)
    {
      StoredFieldInfo result;
      StoredTypeInfo currentType = type;
      do {
        result = currentType.Fields.SingleOrDefault(f => f.Name==name);
        currentType = currentType.Ancestor;
      } while (includeInheritedFields && result==null && currentType!=null);
      if (result == null)
        throw FieldIsNotFound(type.Name, name);
      return result;
    }

    private FieldInfo ResolveNewField(TypeInfo type, string name, bool includeInheritedFields)
    {
      var result = type.Fields
        .SingleOrDefault(f => f.Name==name && (f.IsDeclared || includeInheritedFields));
      if (result == null)
        throw FieldIsNotFound(type.UnderlyingType.GetFullName(), name);
      return result;
    }

    private FieldInfo ResolveNewField(StoredFieldInfo field)
    {
      FieldInfo result;
      return fieldMapping.TryGetValue(field, out result)
        ? result
        : ResolveNewType(field.DeclaringType).Fields.Single(f => f.IsDeclared && f.Name==field.Name);
    }

    #endregion

    private IEnumerable<Hint> GenerateRenameTableHints()
    {
      foreach (var mapping in forwardTypeMapping) {
        var oldTable = mapping.Key.MappingName;
        var newTable = mapping.Value.MappingName;
        if (oldTable != newTable)
          yield return new RenameHint(GetTableName(oldTable), GetTableName(newTable));
      }      
    }

    private IEnumerable<Hint> GenerateRenameColumnHints()
    {
      foreach (var mapping in fieldMapping.Where(m => m.Key.IsPrimitive && m.Key.DeclaringType.IsEntity)) {
        var oldTable = mapping.Key.DeclaringType.MappingName;
        var newTable = mapping.Value.DeclaringType.MappingName;
        var oldColumn = mapping.Key.MappingName;
        var newColumn = mapping.Value.MappingName;
        if (oldColumn != newColumn)
          yield return new RenameHint(GetColumnName(oldTable, oldColumn), GetColumnName(newTable, newColumn));
      }
    }

    private IEnumerable<Hint> GenerateCopyColumnHints(CopyFieldHint hint)
    {
      // searching for all required objects
      var sourceType = ResolveOldType(hint.SourceType);
      var sourceField = ResolveOldField(sourceType, hint.SourceField, true);
      var destinationTypeName = hint.DestinationType.GetFullName();
      var destinationType = ResolveNewType(destinationTypeName);
      var destinationField = ResolveNewField(destinationType, hint.DestinationField, true);
      var sourceHierarchy = sourceType.Hierarchy;
      var destinationHierarchy = destinationType.Hierarchy;

      if (!sourceField.IsPrimitive || !destinationField.IsPrimitive)
        throw new NotImplementedException();

      // checking that types have hierarchies
      if (sourceHierarchy==null)
        throw TypeIsNotInHierarchy(hint.SourceType);
      if (destinationHierarchy == null)
        throw TypeIsNotInHierarchy(destinationTypeName);
      
      // building sets of key columns
      var sourceKeys = sourceHierarchy.Root.Fields
        .Where(f => f.IsPrimaryKey)
        .ToDictionary(
          f => f.Name,
          f => new Pair<string>(f.MappingName, f.ValueType));
      var destinationKeys = destinationHierarchy.Root.Fields
        .Where(f => f.IsPrimaryKey)
        .ToDictionary(
          f => f.Name,
          f => new Pair<string>(f.MappingName, f.ValueType.GetFullName()));

      // checking that keys match: all key columns must have the same name and the same type.
      if (sourceKeys.Count != destinationKeys.Count)
        throw KeysDoNotMatch(sourceType.Name, destinationTypeName);
      var keyPairs = new List<Pair<string>>();
      foreach (var sourceKey in sourceKeys) {
        Pair<string> destinationKey;
        var mismatch =
          !destinationKeys.TryGetValue(sourceKey.Key, out destinationKey) ||
            destinationKey.Second!=sourceKey.Value.Second; // comparing types
        if (mismatch)
          throw KeysDoNotMatch(sourceType.Name, destinationTypeName);
        keyPairs.Add(new Pair<string>(sourceKey.Value.First, destinationKey.First));
      }

      // building destination table names
      var destinationTables = EnumerableUtils.One(destinationType.MappingName);
      if (destinationHierarchy.Schema==InheritanceSchema.ConcreteTable)
        destinationTables = destinationTables
          .Concat(destinationType.GetDescendants(true).Select(type => type.MappingName));

      // generating result hints
      var sourceTable = sourceType.MappingName;
      foreach (var destinationTable in destinationTables) {
        var identityParameters = keyPairs.Select(pair => new IdentityPair(
            GetColumnName(sourceTable, pair.First),
            GetColumnName(destinationTable, pair.Second), false));
        var copyDataHint = new CopyDataHint {SourceTablePath = GetTableName(sourceTable)};
        copyDataHint.CopiedColumns.Add(new Pair<string>(
          GetColumnName(sourceTable, sourceField.MappingName),
          GetColumnName(destinationTable, destinationField.MappingName)));
        copyDataHint.Identities.AddRange(identityParameters);
        yield return copyDataHint;
      }
    }

    // Constructors

    public HintTranslator(StoredDomainModel oldModel, DomainModel newModel)
    {
      this.oldModel = oldModel;
      this.newModel = newModel;
    }

    #region Static helpers

    private static string GetTableName(string name)
    {
      return string.Format("Tables/{0}", name);
    }

    private static string GetColumnName(string tableName, string columnName)
    {
      return string.Format("Tables/{0}/Columns/{1}", tableName, columnName);
    }

    private static InvalidOperationException TypeIsNotFound(string name)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXIsNotFound, name));
    }

    private static InvalidOperationException FieldIsNotFound(string typeName, string fieldName)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExFieldXYIsNotFound, typeName, fieldName));
    }

    private static InvalidOperationException HintConflict(UpgradeHint hintOne, UpgradeHint hintTwo)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExHintXIsConflictingWithHintY, hintOne, hintTwo));
    }

    private static InvalidOperationException KeysDoNotMatch(string typeOne, string typeTwo)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExKeyOfXDoesNotMatchKeyOfY, typeOne, typeTwo));
    }

    private static InvalidOperationException TypeIsNotInHierarchy(string type)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXMustBelongToHierarchy, type));
    }

    #endregion
  }
}