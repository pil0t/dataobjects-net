// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.25

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.StringKeyTestModel;

namespace Xtensive.Storage.Tests.Model.StringKeyTestModel
{
  [HierarchyRoot]
  public class Product : Entity
  {
    [Key, Field(Length = 100)]
    public string Code { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }

    public Product(string code) : base(code) { }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  public class StringKeyTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Product));
      return config;
    }

    [Test]
    public void MainTest()
    {
    }
  }
}