// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class DoubleArithmetic
    : ArithmeticBase<double>
  {
    private const double zero = 0d;
    private const double one = 1d;

    /// <inheritdoc/>
    public override double Add(double value1, double value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return value1 + value2;
        }
      }
      else {
        checked{
          return value1 + value2;
        }
      }
    }

    /// <inheritdoc/>
    public override double Negation(double value)
    {
      if (OverflowAllowed) {
        unchecked{
          return -1*value;
        }
      }
      else {
        checked{
          return -1*value;
        }
      }
    }

    /// <inheritdoc/>
    public override double Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override double One
    {
      get { return one; }
    }

    /// <inheritdoc/>
    public override double Multiply(double value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return value*factor;
        }
      }
      else {
        checked{
          return value*factor;
        }
      }
    }

    /// <inheritdoc/>
    public override double Divide(double value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return value/factor;
        }
      }
      else {
        checked{
          return value/factor;
        }
      }
    }

    /// <inheritdoc/>
    public override double Subtract(double value1, double value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return value1 - value2;
        }
      }
      else {
        checked{
          return value1 - value2;
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<double> CreateNew(ArithmeticRules rules)
    {
      return new DoubleArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public DoubleArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}