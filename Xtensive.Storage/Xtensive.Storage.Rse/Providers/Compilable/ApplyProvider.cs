// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.16

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that iterates over <see cref="BinaryProvider.Right"/> provider result for each item from the <see cref="BinaryProvider.Left"/> provider.
  /// </summary>
  [Serializable]
  public sealed class ApplyProvider : BinaryProvider
  {
    /// <summary>
    /// Gets the apply parameter.
    /// </summary>
    /// <value>The apply parameter.</value>
    public ApplyParameter ApplyParameter { get; private set; }
    
    /// <summary>
    /// Gets apply type.
    /// </summary>
    /// <value>The apply type.</value>
    public ApplyType ApplyType { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      switch (ApplyType) {
        case ApplyType.Cross:
        case ApplyType.Outer:
          return base.BuildHeader();
        case ApplyType.Existing:
        case ApplyType.NotExisting:
          return Left.Header;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return string.Format("{0} apply", ApplyType);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ApplyProvider(ApplyParameter applyParameter, CompilableProvider left, CompilableProvider right)
      : this(applyParameter, left, right, ApplyType.Cross)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ApplyProvider(ApplyParameter applyParameter, CompilableProvider left, CompilableProvider right, ApplyType applyType)
      : base(ProviderType.Apply, left, right)
    {
      ApplyParameter = applyParameter;
      ApplyType = applyType;
    }
  }
}