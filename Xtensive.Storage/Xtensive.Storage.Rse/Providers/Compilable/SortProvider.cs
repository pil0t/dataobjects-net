// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that declares sort operation over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SortProvider : UnaryProvider
  {
    private RecordSetHeader header;

    /// <summary>
    /// Sort order columns indexes.
    /// </summary>
    public DirectionCollection<int> SortOrder { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    protected override void Initialize()
    {
      header = new RecordSetHeader(Source.Header.TupleDescriptor, Source.Header.Columns, Source.Header.OrderDescriptor.Descriptor, Source.Header.Groups, SortOrder);
    }

    public override string GetStringParameters()
    {
      return SortOrder
        .Select(pair => Header.Columns[pair.Key].Name + (pair.Value == Direction.Negative ? " desc" : string.Empty))
        .ToCommaDelimitedString();
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SortProvider(CompilableProvider provider, DirectionCollection<int> tupleSortOrder)
      : base(provider)
    {
      SortOrder = tupleSortOrder;
      Initialize();
    }
  }
}