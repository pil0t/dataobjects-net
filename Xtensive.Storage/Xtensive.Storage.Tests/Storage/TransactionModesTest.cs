// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.30

using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public sealed class TransactionModesTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    [Test]
    public void DefaultTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions};
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.All<Product>().First();
        product.ReorderLevel++;
        Session.Current.Persist();
        var dbTransaction = StorageTestHelper.GetNativeTransaction();
        product.ReorderLevel++;
        Session.Current.Persist();
        Assert.AreSame(dbTransaction, StorageTestHelper.GetNativeTransaction());
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
        tx.Complete();
      }

      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }
    
    [Test]
    public void NotActivatedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions};
      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        tx.Complete();
      }
    }

    [Test]
    public void RollBackedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions};
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
      }

      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.All<Product>().First();
        reorderLevel = product.ReorderLevel;
        product.ReorderLevel++;
        productKey = product.Key;
      }

      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }
    
    [Test]
    public void AutoTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoShortenTransactions
      };
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.All<Product>().Where(p => p.Id > 0).First();
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
      }

      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }

    [Test]
    public void AmbientTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoShortenTransactions | SessionOptions.AmbientTransactions
      };
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.All<Product>().Where(p => p.Id > 0).First();
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
        Assert.IsNotNull(StorageTestHelper.GetNativeTransaction());
        Session.Current.CommitAmbientTransaction();
      }

      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }

    [Test]
    public void NestedTransactionsWithAmbientOptionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AmbientTransactions
      };

      using (var session = Session.Open(Domain, sessionConfiguration))
        try {
          var anton = Query.Single<Customer>("ANTON");
          var firstTransaction = Transaction.Current;
          using (Transaction.Open(TransactionOpenMode.Auto)) {
            var lacor = Query.Single<Customer>("LACOR");
            Assert.AreSame(firstTransaction, Transaction.Current);
          }
          using (Transaction.Open(TransactionOpenMode.New)) {
            var bergs = Query.Single<Customer>("BERGS");
            Assert.AreNotSame(firstTransaction, Transaction.Current);
          }
        }
        finally {
          session.RollbackAmbientTransaction();
        }

      using (var session = Session.Open(Domain, sessionConfiguration))
        try {
          Transaction outerTransaction;
          using (Transaction.Open(TransactionOpenMode.New)) {
            Assert.IsTrue(Transaction.Current.IsNested);
            outerTransaction = Transaction.Current.Outer;
          }
          Assert.AreSame(outerTransaction, Transaction.Current);
        }
        finally {
          session.RollbackAmbientTransaction();
        }
    }

    [Test]
    public void NestedTransactionsWithAutoshortenedOptionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoShortenTransactions
      };
      using (Session.Open(Domain, sessionConfiguration)) {
        using (var outer = Transaction.Open(TransactionOpenMode.New)) {
          Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var mid = Transaction.Open(TransactionOpenMode.New)) {
            Assert.IsFalse(outer.Transaction.IsActuallyStarted);
            Assert.IsFalse(mid.Transaction.IsActuallyStarted);
            using (var inner = Transaction.Open(TransactionOpenMode.New)) {
              Assert.IsFalse(outer.Transaction.IsActuallyStarted);
              Assert.IsFalse(mid.Transaction.IsActuallyStarted);
              Assert.IsFalse(inner.Transaction.IsActuallyStarted);
            }
          }
        }
      }

      using (Session.Open(Domain, sessionConfiguration)) {
        using (var outer = Transaction.Open(TransactionOpenMode.New)) {
          Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var inner = Transaction.Open(TransactionOpenMode.New)) {
            var lacor = Query.Single<Customer>("LACOR");
            Assert.IsTrue(outer.Transaction.IsActuallyStarted);
            Assert.IsTrue(inner.Transaction.IsActuallyStarted);
          }
        }
      }
    }
  }
}