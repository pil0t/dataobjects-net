// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class EntityGroupPrefetchTask
  {
    private static readonly object indexSeekCachingRegion = new object();
    private static readonly Parameter<Tuple> seekParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private static readonly IEqualityComparer<HashSet<int>> columnIndexesComparer =
      HashSet<int>.CreateSetComparer();

    private readonly int[] columnIndexes;
    private Dictionary<Key, bool> keys;
    private readonly TypeInfo type;
    private readonly int cachedHashCode;
    private readonly PrefetchProcessor processor;
    private List<QueryTask> queryTasks;
    private HashSet<Key> foundedEntityKeys;

    public RecordSet RecordSet { get; private set; }

    public void AddKey(Key key, bool exactType)
    {
      if (keys == null)
        keys = new Dictionary<Key, bool>();
      if (keys.ContainsKey(key))
        return;
      keys.Add(key, exactType);
    }

    public void RegisterQueryTasks()
    {
      queryTasks = new List<QueryTask>(keys.Count);
      foreach (var pair in keys) {
        var queryTask = CreateQueryTask(pair.Key);
        queryTasks.Add(queryTask);
        processor.Owner.Session.RegisterDelayedQuery(queryTask);
      }
    }

    public void UpdateCache()
    {
      foundedEntityKeys = new HashSet<Key>();
      var reader = processor.Owner.Session.Domain.RecordSetReader;
      foreach (var queryTask in queryTasks)
        PutLoadedStatesToCache(queryTask.Result, reader);
      HandleMissedKeys();
    }

    public IEnumerable<QueryTask> GetResult()
    {
      return queryTasks;
    }

    public bool Equals(EntityGroupPrefetchTask other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (!type.Equals(other.type))
        return false;
      if (columnIndexes.Length != other.columnIndexes.Length)
        return false;
      for (var i = columnIndexes.Length - 1; i >= 0; i--)
        if (columnIndexes[i] != other.columnIndexes[i])
          return false;
      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (EntityGroupPrefetchTask))
        return false;
      return Equals((EntityGroupPrefetchTask) obj);
    }

    public override int GetHashCode()
    {
      return cachedHashCode;
    }

    private QueryTask CreateQueryTask(Key entityKey)
    {
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate()) {
        seekParameter.Value = entityKey.Value;
        RecordSet = (RecordSet) processor.Owner.Session.Domain.GetCachedItem(
          new Pair<object, EntityGroupPrefetchTask>(indexSeekCachingRegion, this), CreateIndexSeekRecordSet);
        var executableProvider = CompilationContext.Current.Compile(RecordSet.Provider);
        return new QueryTask(executableProvider, parameterContext);
      }
    }

    private static RecordSet CreateIndexSeekRecordSet(object cachingKey)
    {
      var pair = (Pair<object, EntityGroupPrefetchTask>) cachingKey;
      var selectedColumnIndexes = pair.Second.columnIndexes;
      return pair.Second.type.Indexes.PrimaryIndex.ToRecordSet().Seek(() => seekParameter.Value)
        .Select(selectedColumnIndexes);
    }

    private void PutLoadedStatesToCache(IEnumerable<Tuple> queryResult, RecordSetReader reader)
    {
      var records = reader.Read(queryResult, RecordSet.Header);
      foreach (var record in records) {
        if (record!=null) {
          var fetchedKey = record.GetKey();
          var tuple = record.GetTuple();
          if (tuple!=null) {
            processor.SaveStrongReference(processor.Owner.RegisterEntityState(fetchedKey, tuple));
            foundedEntityKeys.Add(fetchedKey);
          }
        }
      }
    }

    private void HandleMissedKeys()
    {
      if (foundedEntityKeys.Count == keys.Count)
        return;
      foreach (var pair in keys) {
        if (!foundedEntityKeys.Contains(pair.Key))
          MarkMissedEntityState(pair.Key, pair.Value);
      }
    }

    private void MarkMissedEntityState(Key key, bool exactType)
    {
      bool isRemoved;
      var cachedEntityState = processor.GetCachedEntityState(key, out isRemoved);
      if (exactType && !isRemoved && (cachedEntityState==null || cachedEntityState.Type==type))
        // Ensures there will be "removed" EntityState associated with this key
        processor.SaveStrongReference(processor.Owner.RegisterEntityState(key, null));
    }


    // Constructors

    public EntityGroupPrefetchTask(TypeInfo type, int[] columnIndexes, PrefetchProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, "columnIndexes");
      ArgumentValidator.EnsureArgumentIsGreaterThan(columnIndexes.Length, 0, "columnIndexes.Length");
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");

      this.type = type;
      this.columnIndexes = columnIndexes;
      this.processor = processor;
      cachedHashCode = 0;
      for (var i = 0; i < columnIndexes.Length; i++)
        cachedHashCode = unchecked (379 * cachedHashCode + columnIndexes[i]);
      cachedHashCode = unchecked (cachedHashCode ^ type.GetHashCode());
    }
  }
}