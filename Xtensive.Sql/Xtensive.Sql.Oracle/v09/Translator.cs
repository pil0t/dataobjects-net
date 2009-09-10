// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Text;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Translator : SqlTranslator
  {
    public override string BatchBegin { get { return "BEGIN\n"; } }
    public override string BatchEnd { get { return "END;\n"; } }
    
    public override string DateTimeFormatString { get { return @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\'\)"; } }
    public override string TimeSpanFormatString { get { return "(INTERVAL '{0}{1} {2}:{3}:{4}.{5:000}' DAY(6) TO SECOND(3))"; } }
    public override string FloatFormatString { get { return base.FloatFormatString + "f"; } }
    public override string DoubleFormatString { get { return base.DoubleFormatString + "d"; } }

    public override void Initialize()
    {
      base.Initialize();
      FloatNumberFormat.NumberDecimalSeparator = ".";
      FloatNumberFormat.NumberGroupSeparator = "";
      FloatNumberFormat.NaNSymbol = "BINARY_FLOAT_NAN";
      FloatNumberFormat.PositiveInfinitySymbol = "+BINARY_FLOAT_INFINITY";
      FloatNumberFormat.NegativeInfinitySymbol = "-BINARY_FLOAT_INFINITY";

      DoubleNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberGroupSeparator = "";
      DoubleNumberFormat.NaNSymbol = "BINARY_DOUBLE_NAN";
      DoubleNumberFormat.PositiveInfinitySymbol = "+BINARY_DOUBLE_INFINITY";
      DoubleNumberFormat.NegativeInfinitySymbol = "-BINARY_DOUBLE_INFINITY";
    }

    public override string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithQuotes(names);
    }

    public override string QuoteString(string str)
    {
      return "N" + base.QuoteString(str);
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
      case SelectSection.HintsEntry:
        return "/*+";
      case SelectSection.HintsExit:
        return "*/";
      default:
        return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlJoinMethod method)
    {
      // TODO: add more hints
      switch (method) {
      case SqlJoinMethod.Loop:
        return "use_nl";
      case SqlJoinMethod.Merge:
        return "use_merge";
      case SqlJoinMethod.Hash:
        return "use_hash";
      default:
        return string.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch(section) {
      case NodeSection.Exit:
        return ".nextval";
      default:
        return string.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    {
      if (node.NodeType==SqlNodeType.Modulo && section==NodeSection.Entry)
        return "MOD(";
      return base.Translate(context, node, section);
    }
    
    public override string Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      if (node.DateTimePart==SqlDateTimePart.Second || node.IntervalPart==SqlIntervalPart.Second)
        switch (section) {
        case ExtractSection.Entry:
          return "TRUNC(EXTRACT(";
        case ExtractSection.Exit:
          return "))";
        default:
          return base.Translate(context, node, section);
        }

      if (node.DateTimePart==SqlDateTimePart.Millisecond || node.IntervalPart==SqlIntervalPart.Millisecond)
        switch (section) {
        case ExtractSection.Entry:
          return "MOD(EXTRACT(";
        case ExtractSection.Exit:
          return ")*1000,1000)";
        default:
          return base.Translate(context, node, section);
        }

      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlDropTable node)
    {
      return "DROP TABLE " + Translate(node.Table) + (node.Cascade ? " CASCADE CONSTRAINTS" : string.Empty);
    }

    public override string Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      return "DROP SEQUENCE " + Translate(node.Sequence);
    }

    public override string Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
      case AlterTableSection.AddColumn:
        return "ADD";
      case AlterTableSection.DropBehavior:
        var cascadableAction = node.Action as SqlCascadableAction;
        if (cascadableAction==null || !cascadableAction.Cascade)
          return string.Empty;
        if (cascadableAction is SqlDropConstraint)
          return "CASCADE";
        if (cascadableAction is SqlDropColumn)
          return "CASCADE CONSTRAINTS";
        throw new ArgumentOutOfRangeException("node.Action");
      default:
        return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, Type literalType, object literalValue)
    {
      switch (Type.GetTypeCode(literalType)) {
      case TypeCode.Boolean:
        return (bool) literalValue ? "1" : "0";
      }
      if (literalType==typeof(byte[])) {
        var values = (byte[]) literalValue;
        var builder = new StringBuilder(2 * (values.Length + 1));
        builder.Append("'");
        builder.AppendHexArray(values);
        builder.Append("'");
        return builder.ToString();
      }
      if (literalType==typeof(Guid))
        return QuoteString(SqlHelper.GuidToString((Guid) literalValue));
      return base.Translate(context, literalType, literalValue);
    }

    public override string Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      return "DROP INDEX " + Translate(node.Index);
    }

    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      /*Index index = node.Index;
            switch (section) {
              case CreateIndexSection.Entry:
                return string.Format("CREATE {0}INDEX {1} ON {2} ("
                  , index.IsUnique ? "UNIQUE " : ""
                  , QuoteIdentifier(index.Name)
                  , Translate(index.DataTable));
              case CreateIndexSection.StorageOptions:
                var builder = new StringBuilder();
                builder.Append(")");
                AppendIndexStorageParameters(builder, index);

                if (!String.IsNullOrEmpty(index.Filegroup)) {
                  builder.Append(" TABLESPACE " + QuoteIdentifier(index.Filegroup));
                }

                //cluster in a separate command
                if (index.IsClustered) {
                  builder.AppendFormat(BatchItemDelimiter + " CLUSTER {0} ON {1}"
                    , QuoteIdentifier(index.Name)
                    , QuoteIdentifier(index.DataTable.Schema.Name, index.DataTable.Name)
                    );
                }
                return builder.ToString();
              case CreateIndexSection.Where:
                return " WHERE ";
              default:
                return string.Empty;
            }*/
      var builder = new StringBuilder();
      var index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          builder.Append("CREATE ");
          if (index.IsUnique)
            builder.Append("UNIQUE ");
          else if (index.IsBitmap)
            builder.Append("BITMAP ");
          builder.Append("INDEX ");
          builder.Append(Translate(index));
          builder.Append(" ON ");
          builder.Append(Translate(index.DataTable));
          return builder.ToString();
        case CreateIndexSection.Exit:
          break;
        case CreateIndexSection.ColumnsEnter:
          return "(";
        case CreateIndexSection.ColumnsExit:
          return ")";
        case CreateIndexSection.NonkeyColumnsEnter:
          break;
        case CreateIndexSection.NonkeyColumnsExit:
          break;
        case CreateIndexSection.StorageOptions:
          break;
        case CreateIndexSection.Where:
          break;
        default:
          throw new ArgumentOutOfRangeException("section");
      }
      return string.Empty;
//      var builder = new StringBuilder();
//      builder.Append("CREATE ");
//      if (index.IsUnique)
//        builder.Append("UNIQUE ");
//      else if (index.IsBitmap)
//        builder.Append("BITMAP ");
//      builder.Append("INDEX ");
//      builder.Append(Translate(index));
//      builder.Append(" ON ");
//      builder.Append(Translate(index.DataTable));
//      builder.Append("(");
//      foreach (var column in index.Columns) {
//        builder.Append(QuoteIdentifier(column.DbName));
//        builder.Append(column.Ascending ? " ASC" : " DESC");
//        builder.Append(RowItemDelimiter);
//      }
//      builder.Length = builder.Length - RowItemDelimiter.Length;
//      builder.Append(")");
//      return builder.ToString();
    }

    public virtual string Translate(Index node)
    {
      return node.DataTable.Schema!=null
        ? QuoteIdentifier(node.DataTable.Schema.DbName, node.DbName)
        : QuoteIdentifier(node.DbName);
    }

    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type==SqlType.Interval)
        return "INTERVAL DAY(6) TO SECOND(3)";
      return base.Translate(type);
    }
    
    public override string Translate(SqlDateTimePart part)
    {
      switch (part) {
      case SqlDateTimePart.DayOfWeek:
      case SqlDateTimePart.DayOfYear:
        throw new NotSupportedException();
      case SqlDateTimePart.Millisecond:
        return "SECOND";
      default:
        return base.Translate(part);
      }
    }

    public override string Translate(SqlIntervalPart part)
    {
      switch (part) {
      case SqlIntervalPart.Millisecond:
        return "SECOND";
      default:
        return base.Translate(part);
      }
    }

    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
      case SqlFunctionType.Truncate:
      case SqlFunctionType.DateTimeTruncate:
        return "TRUNC";
      case SqlFunctionType.IntervalNegate:
        return "-1*";
      default:
        return base.Translate(type);
      }
    }

    public override string Translate(SqlNodeType type)
    {
      switch (type) {
      case SqlNodeType.Modulo:
        return ",";
      case SqlNodeType.DateTimePlusInterval:
        return "+";
      case SqlNodeType.DateTimeMinusInterval:
      case SqlNodeType.DateTimeMinusDateTime:
        return "-";
      default:
        return base.Translate(type);
      }
    }

    public override string Translate(SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared) || lockType.Supports(SqlLockType.SkipLocked))
        return base.Translate(lockType);
      return lockType.Supports(SqlLockType.ThrowIfLocked)
        ? "FOR UPDATE NOWAIT"
        : "FOR UPDATE";
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}