// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.01

using System;
using System.Collections.Generic;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using System.Linq;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected session handler.
  /// </summary>
  public sealed class DisconnectedSessionHandler : ChainingSessionHandler
  {
    private readonly DisconnectedState disconnectedState;
    
    #region Transactions

    /// <inheritdoc/>
    public override bool TransactionIsStarted {
      get { return disconnectedState.IsLocalTransactionOpen; }
    }

    /// <inheritdoc/>
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      disconnectedState.OnTransactionOpened();
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(string name)
    {
      disconnectedState.OnTransactionOpened();
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      disconnectedState.OnTransactionRollbacked();
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      disconnectedState.OnTransactionCommited();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      if (chainedHandler.TransactionIsStarted && !disconnectedState.IsAttachedWhenTransactionWasOpen)
        chainedHandler.CommitTransaction();
      disconnectedState.OnTransactionCommited();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      if (chainedHandler.TransactionIsStarted && !disconnectedState.IsAttachedWhenTransactionWasOpen)
        chainedHandler.RollbackTransaction();
      disconnectedState.OnTransactionRollbacked();
    }

    public void BeginChainedTransaction()
    {
      if (chainedHandler.TransactionIsStarted)
        return;
      if (!disconnectedState.IsConnected)
        throw new ConnectionRequiredException();
      chainedHandler.BeginTransaction(Session.Configuration.DefaultIsolationLevel);
    }

    public void CommitChainedTransaction()
    {
      // We assume that chained transactions are always readonly, so there is no rollback.
      if (chainedHandler.TransactionIsStarted && !disconnectedState.IsAttachedWhenTransactionWasOpen)
        chainedHandler.CommitTransaction();
    }


    #endregion

    internal override bool TryGetEntityState(Key key, out EntityState entityState)
    {
      if (TryGetEntityStateFromSessionCache(key, out entityState))
        return true;
      
      var cachedEntityState = disconnectedState.GetEntityState(key);
      if (cachedEntityState!=null && cachedEntityState.IsLoadedOrRemoved) {
        var tuple = cachedEntityState.Tuple!=null ? cachedEntityState.Tuple.Clone() : null;
        entityState = UpdateEntityStateInSessionCache(key, tuple, true);
        return true;
      }
      entityState = null;
      return false;
    }

    internal override bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      if (TryGetEntitySetStateFromSessionCache(key, fieldInfo, out entitySetState))
        return true;
      
      var cachedState = disconnectedState.GetEntityState(key);
      if (cachedState!=null) {
        var setState = cachedState.GetEntitySetState(fieldInfo);
        entitySetState = UpdateEntitySetStateInSessionCache(key, fieldInfo, setState.Items.Keys, setState.IsFullyLoaded);
        return true;
      }
      entitySetState = null;
      return false;
    }

    internal override EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      var cachedEntityState = tuple==null 
        ? disconnectedState.GetEntityState(key) 
        : disconnectedState.RegisterEntityState(key, tuple);
     
      if (cachedEntityState!=null) {
        if (!cachedEntityState.Key.HasExactType)
          cachedEntityState.Key.TypeReference = key.TypeReference;
      }

      if (cachedEntityState==null || cachedEntityState.IsRemoved || cachedEntityState.Tuple == null)
        return UpdateEntityStateInSessionCache(key, null, true);
      
      var entityState = UpdateEntityStateInSessionCache(cachedEntityState.Key, cachedEntityState.Tuple.Clone(), true);
      
      // Fetch version roots
      if (entityState.Type.HasVersionRoots) {
        BeginChainedTransaction();
        var entity = entityState.Entity as IHasVersionRoots;
        if (entity!=null)
          entity.GetVersionRoots().ToList();
      }
      return entityState;
    }

    internal override EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      var cachedOwner = disconnectedState.GetEntityState(key);
      if (cachedOwner==null || cachedOwner.IsRemoved)
        return null;

      // Merge with disconnected state cache
      var cachedState = disconnectedState.RegisterEntitySetState(key, fieldInfo, isFullyLoaded, entityKeys, auxEntities);
      
      // Update session cache
      return UpdateEntitySetStateInSessionCache(key, fieldInfo, cachedState.Items.Keys, cachedState.IsFullyLoaded);
    }

    /// <inheritdoc/>
    public override EntityState FetchEntityState(Key key)
    {
      var cachedState = disconnectedState.GetEntityState(key);
      
      // If state is cached, let's return it
      if (cachedState!=null && cachedState.IsLoadedOrRemoved) {
        Tuple tuple = null;
        if (!cachedState.IsRemoved && cachedState.Tuple!=null) 
          tuple = cachedState.Tuple.Clone();
        var entityState = Session.UpdateEntityState(cachedState.Key, tuple, true);
        return cachedState.IsRemoved ? null : entityState;
      }

      // If state isn't cached, let's try to to get it from storage
      if ((cachedState!=null && !cachedState.IsLoadedOrRemoved) || disconnectedState.IsConnected) {
        BeginChainedTransaction();
        var type = key.TypeReference.Type;
        Prefetch(key, type, PrefetchHelper.CreateDescriptorsForFieldsLoadedByDefault(type));
        ExecutePrefetchTasks(true);
        EntityState result;
        return TryGetEntityState(key, out result) ? result : null;
      }

      // If state unknown return null
      return null;
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      registry.GetItems(PersistenceState.New)
        .ForEach(item => disconnectedState.Persist(item, PersistActionKind.Insert));
      registry.GetItems(PersistenceState.Modified)
        .ForEach(item => disconnectedState.Persist(item, PersistActionKind.Update));
      registry.GetItems(PersistenceState.Removed)
        .ForEach(item => disconnectedState.Persist(item, PersistActionKind.Remove));
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      Session.ExecuteDelayedQueries(skipPersist); // Important!
      return base.ExecutePrefetchTasks(skipPersist);
    }

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      BeginChainedTransaction();
      base.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      BeginChainedTransaction();
      return base.CreateEnumerationContext();
    }

    /// <inheritdoc/>
    public override IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ManyToOne:
        case Multiplicity.ZeroToOne:
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          Session.Persist(PersistReason.DisconnectedStateReferenceCacheLookup);
          var list = new List<ReferenceInfo>();
          var state = disconnectedState.GetEntityState(target.Key);
          if (state!=null) {
            foreach (var reference in state.GetReferences(association.OwnerField)) {
              var item = FetchEntityState(reference.Key);
              if (item!=null) // Can be already removed (TODO: check this)
                list.Add(new ReferenceInfo(item.Entity, target, association));
            }
          }
          return list;
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany:
          var key = target.GetReferenceKey(association.Reversed.OwnerField);
          if (key!=null)
            return EnumerableUtils.One(new ReferenceInfo(FetchEntityState(key).Entity, target, association));
          return EnumerableUtils<ReferenceInfo>.Empty;
        default:
          throw new ArgumentOutOfRangeException("association.Multiplicity");
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedSessionHandler(SessionHandler chainedHandler, DisconnectedState disconnectedState)
      : base(chainedHandler)
    {
      this.disconnectedState = disconnectedState;
    }
  }
}