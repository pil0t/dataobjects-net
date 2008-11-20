// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.20

using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.BugReports.Bug0004_Model;

namespace Xtensive.Storage.Tests.BugReports.Bug0004_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class SqlTaskEntity : Entity
  {
    [Field]
    public long ID { get; private set; }

    [Field]
    public int Field1 { get; set; }

    [Field]
    public int Field2 { get; set; }

    [Field]
    public int Field3 { get; set; }

    [Field]
    public int Field4 { get; set; }

    [Field]
    public int Field5 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.BugReports
{
  public class Bug0004_SqlBuilderRequestTastBug : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SqlTaskEntity).Assembly, typeof (SqlTaskEntity).Namespace);
      return config;
    }

    [Test]
    public void SqlTaskTest()
    {
      using (Domain.OpenSession()) {
        SqlTaskEntity e1;
        using (TransactionScope trs = Transaction.Open()) {
          e1 = new SqlTaskEntity();
          //insert
          Session.Current.Persist();
          e1.Field5 = 5;
          //update
          trs.Complete();
        }
        using (TransactionScope trs = Transaction.Open()) {
          e1.Field1 = 1;
          e1.Field3 = 3;
          //update
          trs.Complete();
        }
        using (TransactionScope trs = Transaction.Open()) {
          Assert.AreEqual(1, e1.Field1);
          Assert.AreEqual(3, e1.Field3);
        }
      }
    }
  }
}