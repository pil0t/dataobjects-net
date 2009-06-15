// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that property is persistent field,
  /// and, optionally, defines its mapping name.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class FieldAttribute : MappingAttribute
  {
    internal bool? isCollatable;
    internal bool? isTranslatable;
    internal int? length;
    internal bool? lazyLoad;
    internal int? scale;
    internal int? precision;
    internal ReferentialAction? referentialAction;

    /// <summary>
    /// Gets or sets the length of the field.
    /// </summary>
    public int Length
    {
      get { return length.HasValue ? length.Value : 0; }
      set { length = value; }
    }

    /// <summary>
    /// Gets or sets the scale of the field.
    /// </summary>
    public int Scale
    {
      get { return scale.HasValue ? scale.Value : 0; }
      set { scale = value; }
    }

    /// <summary>
    /// Gets or sets the precision of the field.
    /// </summary>
    public int Precision
    {
      get { return precision.HasValue ? precision.Value : 0; }
      set { precision = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is translatable.
    /// </summary>
    public bool Translatable
    {
      get { return isTranslatable.HasValue ? isTranslatable.Value : false; }
      set { isTranslatable = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is collatable.
    /// </summary>
    public bool Collatable
    {
      get { return isCollatable.HasValue ? isCollatable.Value : false; }
      set { isCollatable = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance should be loaded on demand.
    /// </summary>
    public bool LazyLoad
    {
      get { return lazyLoad.HasValue ? lazyLoad.Value : false; }
      set { lazyLoad = value; }
    }

    /// <summary>
    /// Gets or sets the referential action that will be executed on referenced Entity removal.
    /// </summary>
    public ReferentialAction OnRemove
    {
      get { return referentialAction.HasValue ? referentialAction.Value : ReferentialAction.Default; }
      set { referentialAction = value; }
    }

    /// <summary>
    /// Indicates that persistent collection or persistent field
    /// is a "mirror" of another ("master") collection or reference field.
    /// </summary>
    /// <remarks>
    /// <note>This attribute can be applied to persistent properties
    /// of <see cref="Entity"/> and <see cref="EntitySet{T}"/> types.</note>
    /// A collection marked by this attribute does not allocate any space in the database,
    /// except it transparently uses a table allocated for master collection, but
    /// flipped horizontally. Moreover, paired and master collections are always
    /// synchronized with each other, so when you change a paired
    /// collection, its master collection changes too, and vice versa. 
    /// Each master collection can have only one paired collection.
    /// </remarks>
    public string PairTo { get; set; }


    // Constructors

    /// <inheritdoc/>
    public FieldAttribute()
    {
    }

    /// <inheritdoc/>
    public FieldAttribute(string mappingName)
      : base(mappingName)
    {
    }
  }
}