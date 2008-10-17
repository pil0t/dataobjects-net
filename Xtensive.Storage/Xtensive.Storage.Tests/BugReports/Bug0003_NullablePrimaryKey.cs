// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.BugReports.Bug0003_Model;

namespace Xtensive.Storage.Tests.BugReports.Bug0003_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class X : Entity
  {
    [Field]
    public int? ID { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.BugReports
{
  [TestFixture]
  public class Bug0003_NullablePrimaryKey : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(X).Namespace);
      return config;
    }

    protected override Domain BuildDomain(Xtensive.Storage.Configuration.DomainConfiguration configuration)
    {
      Domain domain = null;
      AssertEx.Throws<AggregateException>(() => domain = base.BuildDomain(configuration));
      return domain;
    }
  }
}