// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using System;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual
{
  [TestFixture]
  public class DomainAndSessionSample
  {
    #region Connection URL examples
    public const string PostrgeSqlUrl = "postgresql://user:password@127.0.0.1:8032/myDatabase?Encoding=Unicode";
    public const string SqlServerUrl = "sqlserver://localhost/myDatabase";
    public const string InMemoryUrl = "memory://localhost/myDatabase";
    #endregion

    [HierarchyRoot]
    public class Person : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    #region Domain sample
    public void Main()
    {
      var configuration = new DomainConfiguration("sqlserver://localhost/MyDatabase");

      // Register all types in specified assembly and namespace
      configuration.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      var domain = Domain.Build(configuration);

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var person = new Person();
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }
    }
    #endregion

    public void CurrentSessionTest()
    {
      var domainConfiguration = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof (Person));
      domainConfiguration.ValidationMode = ValidationMode.OnDemand;
      var domain = Domain.Build(domainConfiguration);

      var personId = 1;

      using (var session = Session.Open(domain)) {

        var newPerson = new Person();
        var fetchedPerson = Query<Person>.Single(personId);

        Console.WriteLine("Our session is current: {0}", Session.Current==session);
        Console.WriteLine("New entity is bound to our session: {0}", newPerson.Session==session);
        Console.WriteLine("Fetched entity is bound to our session: {0}", fetchedPerson.Session==session);
      }
    }

    [Test]
    public void SessionConfigurationTest()
    {
//      var domainConfig = DomainConfiguration.Load("TestDomain");
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfig.Types.Register(typeof (Person));
      domainConfig.ValidationMode = ValidationMode.OnDemand;
      var domain = Domain.Build(domainConfig);

      var sessionCongfigOne = new SessionConfiguration {
        BatchSize = 25,
        DefaultIsolationLevel = IsolationLevel.ReadCommitted,
        CacheSize = 1000,
        Options = SessionOptions.AutoShortenTransactions
      };


      var domainConfig = DomainConfiguration.Load("TestDomain");
      var sessionConfigTwo = domainConfig.Sessions["TestSession"];

      Assert.AreEqual(sessionConfigTwo.BatchSize, sessionCongfigOne.BatchSize);
      Assert.AreEqual(sessionConfigTwo.DefaultIsolationLevel, sessionCongfigOne.DefaultIsolationLevel);
      Assert.AreEqual(sessionConfigTwo.CacheSize, sessionCongfigOne.CacheSize);
      Assert.AreEqual(sessionConfigTwo.Options, sessionCongfigOne.Options);

//      using (var session = Session.Open(domain, sessionConfigTwo)) {
//        var newPerson = new Person();
//        var fetchedPerson = Query<Person>.Single(1);
//
//        Console.WriteLine("Our session is current: {0}", Session.Current==session);
//        Console.WriteLine("New entity is bound to our session: {0}", newPerson.Session==session);
//        Console.WriteLine("Fetched entity is bound to our session: {0}", fetchedPerson.Session==session);
//      }
    }

    #region Session sample
    public void SessionSample(Domain domain)
    {
      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var person = new Person();
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }
    }
    #endregion

  }
}
