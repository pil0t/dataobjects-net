// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// Ignore node hint. 
  /// Add possibilities to ignore specified node in comparison.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Path}")]
  public class IgnoreHint : Hint
  {
    /// <summary>
    /// Gets ignored node path.
    /// </summary>
    public string Path { get; private set; }

    /// <inheritdoc/>
    public override IEnumerable<HintTarget> GetTargets()
    {
      yield return new HintTarget(ModelType.Source, Path);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="path">The ignored node path.</param>
    /// <param name="recursively">if set to <see langword="true"/> node properties will be ignored.</param>
    public IgnoreHint(string path)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(path, "path");
      Path = path;
    }
  }
}