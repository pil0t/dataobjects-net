// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="Dictionary{TKey,TValue}"/>-based 
  /// <see cref="INamedValueCollection"/> implementation.
  /// </summary>
  public class NamedValueCollection : INamedValueCollection
  {
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    /// <inheritdoc/>
    public virtual object Get(string name)
    {
      object result;
      if (values.TryGetValue(name, out result))
        return result;
      return null;
    }

    /// <inheritdoc/>
    public virtual void Set(string name, object value)
    {
      values[name] = value;
      if (value==null)
        values.Remove(name);
    }
  }
}