// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Linq;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public class ConstantExtractorTest
  {
    [Test]
    public void SimpleTest()
    {
      var ex = new ConstantExtractor(CreateAdd(1));
      var ey = new ConstantExtractor(CreateAdd(2));

      var tx = ex.Process().ToExpressionTree();
      var ty = ey.Process().ToExpressionTree();

      var cx = (int) ex.GetConstants()[0];
      var cy = (int) ey.GetConstants()[0];

      Assert.AreEqual(cx, 1);
      Assert.AreEqual(cy, 2);
      Assert.AreEqual(tx, ty);
    }
    
    private static Expression<Func<int, int>> CreateAdd(int n)
    {
      var parameter = Expression.Parameter(typeof (int), "x" + n);
      return (Expression<Func<int, int>>) Expression.Lambda(
        Expression.Add(parameter, Expression.Constant(n)), parameter
        );
    }
  }
}