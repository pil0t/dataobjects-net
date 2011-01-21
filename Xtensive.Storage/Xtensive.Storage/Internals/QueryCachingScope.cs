// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using Xtensive.Core;
using Xtensive.Core.IoC;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;

namespace Xtensive.Storage.Internals
{
  internal sealed class QueryCachingScope : SimpleScope<QueryCachingScope.Variator>
  {
    internal abstract class Variator {}

    private TranslatedQuery parameterizedQuery;

    public static new QueryCachingScope Current {
      get { return (QueryCachingScope) SimpleScope<Variator>.Current; }
    }

    public Parameter QueryParameter { get; private set; }
    public ExtendedExpressionReplacer QueryParameterReplacer { get; private set; }
    public bool Execute { get; private set; }

    /// <exception cref="NotSupportedException">Second attempt to set this property.</exception>
    public TranslatedQuery ParameterizedQuery {
      get { return parameterizedQuery; }
      set {
        if (parameterizedQuery != null)
          throw Exceptions.AlreadyInitialized("ParameterizedQuery");
        parameterizedQuery = value;
      }
    }

  
    // Constructors

    public QueryCachingScope(Parameter queryParameter, ExtendedExpressionReplacer queryParameterReplacer)
      : this(queryParameter, queryParameterReplacer, true)
    {
    }

    public QueryCachingScope(Parameter queryParameter, ExtendedExpressionReplacer queryParameterReplacer, bool execute)
    {
      QueryParameter = queryParameter;
      QueryParameterReplacer = queryParameterReplacer;
      Execute = execute;
    }
  }
}