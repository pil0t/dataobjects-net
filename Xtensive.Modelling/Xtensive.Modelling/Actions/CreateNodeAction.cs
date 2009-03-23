// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Resources;
using System.Linq;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Describes node creation.
  /// </summary>
  [Serializable]
  public class CreateNodeAction : NodeAction
  {
    private Type type;
    private string name;
    private int index;
    private object[] parameters;

    public Type Type {
      get { return type; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        type = value;
      }
    }

    public string Name {
      get { return name; }
      set {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this.EnsureNotLocked();
        name = value;
      }
    }

    public int Index {
      get { return index; }
      set {
        this.EnsureNotLocked();
        index = value;
      }
    }

    public object[] Parameters {
      get {
        if (!IsLocked)
          return parameters;
        return parameters==null ? null : (object[]) parameters.Clone();
      }
      set {
        this.EnsureNotLocked();
        parameters = value;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Required constructor isn't found.</exception>
    protected override void PerformApply(IModel model, IPathNode item)
    {
      var node = (Node) item;
      if (TryConstructor(node, name, index))
        return;
      if (TryConstructor(node, name))
        return;
      if (TryConstructor(node, index))
        return;
      throw new InvalidOperationException(string.Format(
        Strings.ExCannotFindConstructorToExecuteX, this));
    }

    protected bool TryConstructor(params object[] args)
    {
      if (parameters!=null)
        args = args.Concat(parameters).ToArray();
      var argTypes = args.Select(a => a.GetType()).ToArray();
      var ci = type.GetConstructor(argTypes);
      if (ci==null)
        return false;
      ci.Invoke(args);
      return true;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Pair<string>> GetParameters()
    {
      foreach (var kvp in base.GetParameters())
        yield return kvp;
      yield return new Pair<string>("Type", type.GetShortName());
      yield return new Pair<string>("Name", name);
      yield return new Pair<string>("Index", index.ToString());
      if (parameters!=null)
        yield return new Pair<string>("Parameters", parameters.ToCommaDelimitedString());
    }
  }
}