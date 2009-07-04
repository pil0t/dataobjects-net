﻿// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.13

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Resources;
using Operator = Xtensive.Core.Reflection.WellKnown.Operator;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class StringMappings
  {
    private static readonly SqlLiteral<string> Percent = SqlFactory.Literal("%");
    
    [Compiler(typeof(string), "StartsWith")]
    public static SqlExpression StringStartsWith(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(_this, SqlFactory.Concat(value, Percent));
    }
  
    [Compiler(typeof(string), "EndsWith")]
    public static SqlExpression StringEndsWith(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(_this, SqlFactory.Concat(Percent, value));
    }

    [Compiler(typeof(string), "Contains")]
    public static SqlExpression StringContains(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Like(_this, SqlFactory.Concat(Percent, SqlFactory.Concat(value, Percent)));
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return SqlFactory.Substring(_this, startIndex);
    }

    [Compiler(typeof(string), "Substring")]
    public static SqlExpression StringSubstring(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
    {
      return SqlFactory.Substring(_this, startIndex, length);
    }

    [Compiler(typeof(string), "ToUpper")]
    public static SqlExpression StringToUpper(SqlExpression _this)
    {
      return SqlFactory.Upper(_this);
    }

    [Compiler(typeof(string), "ToLower")]
    public static SqlExpression StringToLower(SqlExpression _this)
    {
      return SqlFactory.Lower(_this);
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression _this)
    {
      return SqlFactory.Trim(_this);
    }

    private static SqlExpression GenericTrim(SqlExpression _this, SqlExpression trimChars, SqlTrimType trimType)
    {
      if (trimChars is SqlNull)
        return SqlFactory.Trim(_this, trimType);
      var exactTrimChars = trimChars as SqlLiteral<char[]>;
      if (exactTrimChars==null)
        throw new NotSupportedException(Strings.ExStringTrimSupportedOnlyWithConstants);
      return exactTrimChars.Value.Length==0
        ? SqlFactory.Trim(_this, trimType)
        : SqlFactory.Trim(_this, trimType, new string(exactTrimChars.Value));
    }

    [Compiler(typeof(string), "Trim")]
    public static SqlExpression StringTrim(SqlExpression _this,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return GenericTrim(_this, trimChars, SqlTrimType.Both);
    }

    [Compiler(typeof(string), "TrimStart")]
    public static SqlExpression StringTrimStart(SqlExpression _this,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return GenericTrim(_this, trimChars, SqlTrimType.Leading);
    }

    [Compiler(typeof(string), "TrimEnd")]
    public static SqlExpression StringTrimEnd(SqlExpression _this,
      [Type(typeof(char[]))] SqlExpression trimChars)
    {
      return GenericTrim(_this, trimChars, SqlTrimType.Trailing);
    }

    [Compiler(typeof(string), "Length", TargetKind.PropertyGet)]
    public static SqlExpression StringLength(SqlExpression _this)
    {
      return SqlFactory.Length(_this);
    }

    [Compiler(typeof(string), "ToString")]
    public static SqlExpression StringToString(SqlExpression _this)
    {
      return _this;
    }

    [Compiler(typeof(string), "Replace")]
    public static SqlExpression StringReplaceCh(SqlExpression _this,
      [Type(typeof(char))] SqlExpression oldChar,
      [Type(typeof(char))] SqlExpression newChar)
    {
      return SqlFactory.Replace(_this, oldChar, newChar);
    }

    [Compiler(typeof(string), "Replace")]
    public static SqlExpression StringReplaceStr(SqlExpression _this,
      [Type(typeof(string))] SqlExpression oldValue,
      [Type(typeof(string))] SqlExpression newValue)
    {
      return SqlFactory.Replace(_this, oldValue, newValue);
    }

    [Compiler(typeof(string), "Insert")]
    public static SqlExpression StringInsert(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.Concat(SqlFactory.Concat(
        SqlFactory.Substring(_this, 0, startIndex), value),
        SqlFactory.Substring(_this, startIndex, SqlFactory.Length(_this) - startIndex));
    }

    [Compiler(typeof(string), "Remove")]
    public static SqlExpression StringRemove(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return SqlFactory.Substring(_this, SqlFactory.Literal(0), startIndex);
    }

    [Compiler(typeof(string), "Remove")]
    public static SqlExpression StringRemove(SqlExpression _this,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression count)
    {
      return SqlFactory.Concat(
        SqlFactory.Substring(_this, SqlFactory.Literal(0), startIndex),
        SqlFactory.Substring(_this, startIndex + count));
    }

    [Compiler(typeof(string), "IsNullOrEmpty", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringIsNullOrEmpty(
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.IsNull(value) || SqlFactory.Length(value)==SqlFactory.Literal(0);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1)
    {
      return SqlFactory.Concat(str0, str1);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1,
      [Type(typeof(string))] SqlExpression str2)
    {
      return SqlFactory.Concat(SqlFactory.Concat(str0, str1), str2);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string))] SqlExpression str0,
      [Type(typeof(string))] SqlExpression str1,
      [Type(typeof(string))] SqlExpression str2,
      [Type(typeof(string))] SqlExpression str3)
    {
      return SqlFactory.Concat(SqlFactory.Concat(SqlFactory.Concat(str0, str1), str2), str3);
    }

    [Compiler(typeof(string), "Concat", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringConcat(
      [Type(typeof(string[]))] SqlExpression values)
    {
      if (values.NodeType!=SqlNodeType.Container)
        throw new NotSupportedException();
      var container = (SqlContainer) values;
      if (container.Value.GetType() != typeof(SqlExpression[]))
        throw new NotSupportedException();
      var expressions = (SqlExpression[]) container.Value;
      if (expressions.Length==0)
        return SqlFactory.Literal("");
      return expressions.Aggregate(SqlFactory.Concat);
    }

    [Compiler(typeof(string), "Compare", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringCompare(
      [Type(typeof(string))] SqlExpression strA,
      [Type(typeof(string))] SqlExpression strB)
    {
      var result = SqlFactory.Case();
      result.Add(strA > strB, SqlFactory.Literal(1));
      result.Add(strA < strB, SqlFactory.Literal(-1));
      result.Else = SqlFactory.Literal(0);
      return result;
    }

    [Compiler(typeof(string), "CompareTo")]
    public static SqlExpression StringCompareTo(SqlExpression _this,
      [Type(typeof(string))] SqlExpression strB)
    {
      return StringCompare(_this, strB);
    }

    private static SqlExpression GenericStringIndexOf(SqlExpression _this,
      SqlExpression substring)
    {
      return SqlFactory.Position(substring, _this);
    }

    private static SqlExpression GenericStringIndexOf(SqlExpression _this,
      SqlExpression substring, SqlExpression startIndex)
    {
      return SqlFactory.Coalesce(startIndex + 
        SqlFactory.NullIf(SqlFactory.Position(substring, SqlFactory.Substring(_this, startIndex)), -1),
        -1);
    }

    private static SqlExpression GenericStringIndexOf(SqlExpression _this,
      SqlExpression substring, SqlExpression startIndex, SqlExpression length)
    {
      return SqlFactory.Coalesce(startIndex + 
        SqlFactory.NullIf(SqlFactory.Position(substring, SqlFactory.Substring(_this, startIndex, length)), -1),
        -1);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfString(SqlExpression _this,
      [Type(typeof(string))] SqlExpression str)
    {
      return GenericStringIndexOf(_this, str);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfString(SqlExpression _this,
      [Type(typeof(string))] SqlExpression str,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return GenericStringIndexOf(_this, str, startIndex);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfString(SqlExpression _this,
      [Type(typeof(string))] SqlExpression str,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
    {
      return GenericStringIndexOf(_this, str, startIndex, length);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfChar(SqlExpression _this,
      [Type(typeof(char))] SqlExpression ch)
    {
      return GenericStringIndexOf(_this, ch);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfChar(SqlExpression _this,
      [Type(typeof(char))] SqlExpression ch,
      [Type(typeof(int))] SqlExpression startIndex)
    {
      return GenericStringIndexOf(_this, ch, startIndex);
    }

    [Compiler(typeof(string), "IndexOf")]
    public static SqlExpression StringIndexOfChar(SqlExpression _this,
      [Type(typeof(char))] SqlExpression ch,
      [Type(typeof(int))] SqlExpression startIndex,
      [Type(typeof(int))] SqlExpression length)
    {
      return GenericStringIndexOf(_this, ch, startIndex, length);
    }

    [Compiler(typeof(string), "Chars", TargetKind.PropertyGet)]
    public static SqlExpression StringChars(SqlExpression _this, [Type(typeof(int))] SqlExpression index)
    {
      return SqlFactory.Substring(_this, index, 1);
    }

    [Compiler(typeof(string), "Equals")]
    public static SqlExpression StringEquals(SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return value is SqlNull ? (SqlExpression) SqlFactory.IsNull(_this) : _this==value;
    }

    [Compiler(typeof(StringExtensions), "LessThan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringLessThan(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.LessThan(_this, value);
    }

    [Compiler(typeof(StringExtensions), "LessThanOrEqual", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringLessThanOrEquals(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.LessThanOrEquals(_this, value);
    }

    [Compiler(typeof(StringExtensions), "GreaterThan", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringGreaterThan(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.GreaterThan(_this, value);
    }

    [Compiler(typeof(StringExtensions), "GreaterThanOrEqual", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringGreaterThanOrEquals(
      [Type(typeof(string))] SqlExpression _this,
      [Type(typeof(string))] SqlExpression value)
    {
      return SqlFactory.GreaterThanOrEquals(_this, value);
    }

    [Compiler(typeof(StringExtensions), "IsNullOrEmpty", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression StringIsNullOrEmptyExtension(
      [Type(typeof(string))] SqlExpression value)
    {
      return StringIsNullOrEmpty(value);
    }

    [Compiler(typeof(Enumerable), "Contains", TargetKind.Static | TargetKind.Method, 1)]
    public static SqlExpression EnumerableContains(MemberInfo member, SqlExpression sequence, SqlExpression value)
    {
      var method = (MethodInfo) member;
      if (method.GetGenericArguments()[0] != typeof(char))
        throw new NotSupportedException();
      return StringContains(sequence, value);
    }

    [Compiler(typeof(string), Operator.Equality, TargetKind.Operator)]
    public static SqlExpression StringOperatorEquality(SqlExpression left, SqlExpression right)
    {
      return SqlFactory.Equals(left, right);
    }

    [Compiler(typeof(string), Operator.Inequality, TargetKind.Operator)]
    public static SqlExpression StringOperatorInequality(SqlExpression left, SqlExpression right)
    {
      return SqlFactory.NotEquals(left, right);
    }
  }
}
