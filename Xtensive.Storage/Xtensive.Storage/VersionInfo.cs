// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using System.Runtime.Serialization;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains information about entity version.
  /// </summary>
  [DebuggerDisplay("{Value}")]
  [Serializable]
  public struct VersionInfo : IEquatable<VersionInfo>
  {
    private const string EmptyStringValue = "Void";
    [NonSerialized]
    private int cachedHashCode;
    [NonSerialized]
    private bool isHashCodeCalculated;
    [NonSerialized]
    private Tuple value;

    private object serializedValue;

    /// <summary>
    /// Gets a value indicating whether this instance is not contains version.
    /// </summary>
    public bool IsVoid
    {
      [DebuggerStepThrough]
      get { return Value==null; }
    }

    internal Tuple Value
    {
      [DebuggerStepThrough]
      get
      {
        if (serializedValue!=null && value==null)
          Deserialize();
        return value;
      }
    }

    /// <summary>
    /// Joins this version tuple with specified key value tuple and specified version tuple.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="versionInfo">The version info.</param>
    /// <returns>New combined tuple.</returns>
    internal VersionInfo Join(Key key, VersionInfo versionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");

      Tuple resultVersion = Value;
      if (resultVersion==null)
        resultVersion = key.Value;
      else
        resultVersion = resultVersion.Combine(key.Value);
      if (!versionInfo.IsVoid)
        resultVersion = resultVersion.Combine(versionInfo.Value);

      return new VersionInfo(resultVersion.ToRegular());
    }

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(VersionInfo other)
    {
      if (IsVoid || other.IsVoid)
        return true;
      return Value.Equals(other.Value);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (IsVoid)
        return false;
      if (ReferenceEquals(obj, null))
        return false;
      if (obj.GetType()!= typeof (VersionInfo))
        return false;
      if (obj.GetHashCode()!=GetHashCode())
        return false;
      return Equals((VersionInfo) obj);
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator ==(VersionInfo left, VersionInfo right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator !=(VersionInfo left, VersionInfo right)
    {
      return !left.Equals(right);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      if (!isHashCodeCalculated) {
        if (!IsVoid)
          cachedHashCode = Value.GetHashCode();
        isHashCodeCalculated = true;
      }
      return cachedHashCode;
    }

    # endregion

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string ToString()
    {
      return Value==null ? EmptyStringValue : Value.ToString(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="version">The version tuple.</param>
    public VersionInfo(Tuple version)
    {
      value = version;
      cachedHashCode = 0;
      isHashCodeCalculated = version==null;
      serializedValue = null;
    }


    // Serialization

    [OnSerializing]
    internal void OnSerializing(StreamingContext context)
    {
      if (Value==null)
        return;
      serializedValue = new SerializedTuple(Value);
    }

    [OnSerialized]
    internal void OnSerialized(StreamingContext context)
    {
      serializedValue = null;
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
      Deserialize();
    }

    private void Deserialize()
    {
      if (serializedValue==null)
        return;
      value = ((SerializedTuple) serializedValue).Value;
      serializedValue = null;
    }
  }
}