// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A binding of a parameter for <see cref="QueryRequest"/>.
  /// </summary>
  public class QueryParameterBinding : ParameterBinding
  {
    public Func<object> ValueAccessor { get; private set; }

    public QueryParameterBindingType BindingType { get; private set; }

    // Constructors

    public QueryParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping, QueryParameterBindingType bindingType)
      : base(typeMapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(valueAccessor, "valueAccessor");

      switch (bindingType) {
      case QueryParameterBindingType.Regular:
      case QueryParameterBindingType.SmartNull:
        ArgumentValidator.EnsureArgumentNotNull(typeMapping, "typeMapping");
        break;
      }

      ValueAccessor = valueAccessor;
      BindingType = bindingType;
    }

    public QueryParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping)
      : this(valueAccessor, typeMapping, QueryParameterBindingType.Regular)
    {
    }
  }
}