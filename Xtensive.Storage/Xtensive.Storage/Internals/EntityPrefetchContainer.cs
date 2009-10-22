// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal abstract class EntityPrefetchContainer
  {
    private static readonly object indexSeekCachingRegion = new object();
    private static readonly Parameter<Tuple> seekParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

    private readonly SortedDictionary<int, ColumnInfo> columns = new SortedDictionary<int, ColumnInfo>();

    protected readonly PrefetchProcessor Processor;

    public Key Key { get; protected set; }

    public readonly bool ExactType;

    public TypeInfo Type { get; protected set; }

    public EntityGroupPrefetchTask Task { get; protected set; }

    protected List<int> ColumnIndexesToBeLoaded { get; set; }

    public abstract EntityGroupPrefetchTask GetTask();

    public void AddColumns(IEnumerable<ColumnInfo> candidateColumns)
    {
      var primaryIndex = Type.Indexes.PrimaryIndex;
      foreach (var column in candidateColumns) {
        if (Type.IsInterface == column.Field.DeclaringType.IsInterface)
          columns[Type.Fields[column.Field.Name].MappingInfo.Offset] = column;
        else if (column.Field.DeclaringType.IsInterface)
          columns[Type.FieldMap[column.Field].MappingInfo.Offset] = column;
        else
          throw new InvalidOperationException();
      }
    }

    protected bool SelectColumnsToBeLoaded()
    {
      var key = Key;
      Tuple tuple;
      if (!Processor.TryGetTupleOfNonRemovedEntity(ref key, out tuple))
        return false;
      var needToFetchSystemColumns = false;
      foreach (var pair in columns)
        if (tuple==null || !tuple.GetFieldState(pair.Key).IsAvailable())
          if (pair.Value.IsPrimaryKey || pair.Value.IsSystem)
            needToFetchSystemColumns = ExactType && tuple==null;
          else {
            if (ColumnIndexesToBeLoaded == null)
              ColumnIndexesToBeLoaded = CreateColumnIndexCollection();
            ColumnIndexesToBeLoaded.Add(pair.Key);
          }
      if (needToFetchSystemColumns && ColumnIndexesToBeLoaded == null)
        ColumnIndexesToBeLoaded = CreateColumnIndexCollection();
      return ColumnIndexesToBeLoaded != null;
    }

    private List<int> CreateColumnIndexCollection()
    {
      var result = new List<int>(columns.Count);
      result.AddRange(Type.Indexes.PrimaryIndex.ColumnIndexMap.System);
      return result;
    }


    // Constructors

    protected EntityPrefetchContainer(Key key, TypeInfo type, bool exactType, PrefetchProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");
      Key = key;
      Type = type;
      ExactType = exactType;
      Processor = processor;
    }
  }
}