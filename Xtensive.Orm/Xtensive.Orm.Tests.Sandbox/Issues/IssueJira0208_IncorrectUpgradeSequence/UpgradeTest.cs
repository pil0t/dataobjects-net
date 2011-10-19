// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using M1 = Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var toRemove = new M1.EntityToRemove();
          var toKeep = new M1.EntityToKeep(toRemove);
          tx.Complete();
        }
      }
    }

    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.PerformSafely);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var toKeep = session.Query.All<M2.EntityToKeep>().Single();
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(M1.EntityToKeep).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}