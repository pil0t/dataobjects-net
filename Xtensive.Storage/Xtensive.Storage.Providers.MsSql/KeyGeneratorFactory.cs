// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <summary>
  /// Generator factory
  /// </summary>
  public sealed class KeyGeneratorFactory : Providers.KeyGeneratorFactory
  {
    /// <inheritdoc/>
    protected override KeyGenerator CreateGenerator<TFieldType>(HierarchyInfo hierarchy)
    {
      DomainHandler dh = (DomainHandler)Handlers.DomainHandler;
      Schema schema = dh.Schema;
      SqlBatch sqlCreate = null;
      Table genTable = schema.Tables[hierarchy.MappingName];
      SqlValueType columnType = dh.ValueTypeMapper.BuildSqlValueType(hierarchy.KeyColumns[0]);

      if (genTable == null) {
        genTable = schema.CreateTable(hierarchy.MappingName);
        var column = genTable.CreateColumn("ID", columnType);
        column.SequenceDescriptor = new SequenceDescriptor(column, hierarchy.KeyGeneratorCacheSize, hierarchy.KeyGeneratorCacheSize);
        sqlCreate = SqlFactory.Batch();
        sqlCreate.Add(SqlFactory.Create(genTable));
      }

      SqlBatch sqlNext = SqlFactory.Batch();
      SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(genTable));
      sqlNext.Add(insert);
      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"), columnType.DataType));
      sqlNext.Add(select);

      return new SqlCachingKeyGenerator<TFieldType>(hierarchy, hierarchy.KeyGeneratorCacheSize, sqlNext, sqlCreate);
    }
  }
}