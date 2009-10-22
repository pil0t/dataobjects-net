// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetcherTest : NorthwindDOModelTest
  {
    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var recreateConfig = configuration.Clone();
      var domain = Domain.Build(configuration);
      DataBaseFiller.Fill(domain);
      return domain;
    }

    [Test]
    public void EnumerableOfNonEntityTest()
    {
      List<Key> keys;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        keys = Query<Order>.All.Select(o => o.Key).ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.Employee);
        var orderType = typeof (Order).GetTypeInfo();
        var employeeField = orderType.Fields["Employee"];
        var employeeType = typeof (Employee).GetTypeInfo();
        Func<FieldInfo, bool> fieldSelector = field => field.IsPrimaryKey || field.IsSystem
          || !field.IsLazyLoad && !field.IsEntitySet;
        foreach (var key in prefetcher) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, key.Type, session, fieldSelector);
          var orderState = session.EntityStateCache[key, true];
          var employeeKey = Key.Create(Domain, typeof(Employee).GetTypeInfo(Domain),
            TypeReferenceAccuracy.ExactType, employeeField.Association.ExtractForeignKey(orderState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session, fieldSelector);
        }
      }
    }

    [Test]
    public void EnumerableOfEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = Query<Order>.All.Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails);
        var orderDetailsField = typeof (Order).GetTypeInfo().Fields["OrderDetails"];
        foreach (var order in prefetcher) {
          EntitySetState entitySetState;
          Assert.IsTrue(session.Handler.TryGetEntitySetState(order.Key, orderDetailsField, out entitySetState));
          Assert.IsTrue(entitySetState.IsFullyLoaded);
        }
      }
    }

    [Test]
    public void PreservingOriginalOrderOfElementsTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Order>.All.ToList();
        var actual = expected.Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PrefetchManyNotFullBatchTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var employeesField = typeof (Territory).GetTypeInfo().Fields["Employees"];
        var ordersField = typeof (Employee).GetTypeInfo().Fields["Orders"];
        var prefetcher = Query<Territory>.All.PrefetchMany(t => t.Employees, employees => employees,
          employees => employees.Prefetch(e => e.Orders));
        foreach (var territory in prefetcher) {
          var entitySetState = GetFullyLoadedEntitySet(session, territory.Key, employeesField);
          foreach (var employeeKey in entitySetState)
            GetFullyLoadedEntitySet(session, employeeKey, ordersField);
        }
        Assert.AreEqual(2, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchManySeveralBatchesTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var detailsField = typeof (Order).GetTypeInfo().Fields["OrderDetails"];
        var productField = typeof (OrderDetails).GetTypeInfo().Fields["Product"];
        var prefetcher = Query<Order>.All.PrefetchMany(o => o.OrderDetails, details => details,
          details => details.Prefetch(d => d.Product));
        foreach (var order in prefetcher) {
          var entitySetState = GetFullyLoadedEntitySet(session, order.Key, detailsField);
          foreach (var detailKey in entitySetState) {
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(detailKey, detailKey.Type, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
            PrefetchTestHelper.AssertReferencedEntityIsLoaded(detailKey, session, productField);
          }
        }
        Assert.AreEqual(47, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchSingleTest()
    {
      List<Key> keys;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open())
        keys = Query<Order>.All.Select(o => o.Key).ToList();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var orderType = typeof (Order).GetTypeInfo();
        var employeeType = typeof (Employee).GetTypeInfo();
        var employeeField = typeof (Order).GetTypeInfo().Fields["Employee"];
        var ordersField = typeof (Employee).GetTypeInfo().Fields["Orders"];
        var prefetcher = keys.Prefetch<Order, Key>(key => key)
          .PrefetchSingle(o => o.Employee, employee => employee,
            employee => employee.Prefetch(e => e.Orders));
        var count = 0;
        foreach (var orderKey in prefetcher) {
          Assert.AreEqual(keys[count], orderKey);
          count++;
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType, session,
            field => PrefetchHelper.IsFieldToBeLoadedByDefault(field)
              || field.Equals(employeeField) || (field.Parent != null && field.Parent.Equals(employeeField)));
          var state = session.EntityStateCache[orderKey, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(state.Entity.GetFieldValue<Employee>(employeeField).Key,
            employeeType, session, field =>
              PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(ordersField));
        }
        Assert.AreEqual(keys.Count, count);
        Assert.GreaterOrEqual(11, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PreservingOrderInPrefetchManyNotFullBatchTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Territory>.All.ToList();
        var actual = expected.PrefetchMany(t => t.Employees, employees => employees,
          employees => employees.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreserveOrderingInPrefetchManySeveralBatchesTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Order>.All.ToList();
        var actual = expected.PrefetchMany(o => o.OrderDetails, details => details,
          details => details.Prefetch(d => d.Product)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleNotFullBatchTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Order>.All.Take(53).ToList();
        var actual = expected.PrefetchSingle(o => o.Employee, employee => employee,
          employee => employee.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleSeveralBatchesTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Order>.All.ToList();
        var actual = expected.PrefetchSingle(o => o.Employee, employee => employee,
          employee => employee.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void InvalidArgumentsTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        AssertEx.Throws<ArgumentException>(() => Query<Territory>.All.Prefetch(t => new {t.Id, t.Region}));
        AssertEx.Throws<ArgumentException>(() => Query<Territory>.All.Prefetch(t => t.Region.RegionDescription));
        AssertEx.Throws<ArgumentException>(() => Query<Territory>.All.Prefetch(t => t.PersistenceState));
      }
    }

    [Test]
    public void SimultaneouslyUsageOfMultipleEnumerators()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var source = Query<Order>.All.ToList();
        var prefetcher = source.Prefetch(o => o.OrderDetails);
        using (var enumerator0 = prefetcher.GetEnumerator()) {
          enumerator0.MoveNext();
          enumerator0.MoveNext();
          enumerator0.MoveNext();
          Assert.IsTrue(source.SequenceEqual(prefetcher));
          var index = 3;
          while (enumerator0.MoveNext())
            Assert.AreSame(source[index++], enumerator0.Current);
          Assert.AreEqual(source.Count, index);
        }
      }
    }
    
    private static EntitySetState GetFullyLoadedEntitySet(Session session, Key key,
      FieldInfo employeesField)
    {
      EntitySetState entitySetState;
      Assert.IsTrue(session.Handler.TryGetEntitySetState(key, employeesField, out entitySetState));
      Assert.IsTrue(entitySetState.IsFullyLoaded);
      return entitySetState;
    }
  }
}