// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.24

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  [Ignore("No such domain configuration for now")]
  public class EncryptedConnectionStringTest : HasConfigurationAccessTest
  {
    [Test]
    public void MainTest()
    {
      var config = LoadDomainConfiguration("encrypted");
      Assert.That(config.ConnectionInfo.ConnectionString, Is.Not.Null.And.Not.Empty);
      Assert.That(config.ConnectionInfo.Provider, Is.Not.Null.And.Not.Empty);
    }
  }
}