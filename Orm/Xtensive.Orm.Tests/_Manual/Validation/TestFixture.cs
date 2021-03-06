// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.28

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Tests._Manual.Validation
{
  [TestFixture]
  public class TestFixture : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        try {
          using (var transactionScope = session.OpenTransaction()) {
            var person = new Person(session) {
              FirstName = "Mike",
              LastName = "Grooovy",
              Height = 1.5,
              BirthDay = new DateTime(1983, 03, 16),
              IsSubscribedOnNews = true,
              Email = "mike@grooovy.com"
            };
            session.Validate();
            transactionScope.Complete();
          }
        }
        catch (ValidationFailedException exception) {
          Console.WriteLine("Following validation errors were found:");
          foreach (var errorInfo in exception.ValidationErrors)
            foreach (var error in errorInfo.Errors)
              Console.WriteLine("{0}: {1}", errorInfo.Target, error.ErrorMessage);
        }
      }
    }
  }
}
