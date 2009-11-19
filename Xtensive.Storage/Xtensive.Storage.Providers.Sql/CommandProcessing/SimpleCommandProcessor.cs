// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A command processor that simply executes all incoming commands immediately.
  /// </summary>
  public sealed class SimpleCommandProcessor : CommandProcessor
  {
    /// <inheritdoc/>
    public override void ExecuteRequests(bool allowPartialExecution)
    {
      ExecuteAllTasks();
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> ExecuteRequestsWithReader(QueryRequest lastRequest)
    {
      ExecuteAllTasks();
      return RunTupleReader(ExecuteQuery(new SqlQueryTask(lastRequest)), lastRequest.TupleDescriptor);
    }

    /// <inheritdoc/>
    public override void ProcessTask(SqlQueryTask task)
    {
      ReadTuples(ExecuteQuery(task), task.Request.TupleDescriptor, task.Output);
    }

    /// <inheritdoc/>
    public override void ProcessTask(SqlPersistTask task)
    {
      ExecutePersist(task);
    }

    #region Private / internal methods

    private void ExecuteAllTasks()
    {
      while (tasks.Count > 0) {
        var task = tasks.Dequeue();
        task.ProcessWith(this);
      }
    }

    private void ReadTuples(DbDataReader reader, TupleDescriptor descriptor, List<Tuple> output)
    {
      var enumerator = RunTupleReader(reader, descriptor);
      using (enumerator) {
        while (enumerator.MoveNext())
          output.Add(enumerator.Current);
      }
    }

    private void ExecutePersist(SqlPersistTask task)
    {
      AllocateCommand();
      try {
        var part = factory.CreatePersistCommandPart(task, DefaultParameterNamePrefix);
        activeCommand.AddPart(part);
        activeCommand.ExecuteNonQuery();
      }
      finally {
        DisposeCommand();
      }
    }

    private DbDataReader ExecuteQuery(SqlQueryTask task)
    {
      AllocateCommand();
      try {
        var part = factory.CreateQueryCommandPart(task, DefaultParameterNamePrefix);
        activeCommand.AddPart(part);
        return activeCommand.ExecuteReader();
      }
      finally {
        DisposeCommand();
      }
    }

    #endregion


    // Constructors

    public SimpleCommandProcessor(
      DomainHandler domainHandler, Session session,
      SqlConnection connection, CommandPartFactory factory)
      : base(domainHandler, session, connection, factory)
    {
    }
  }
}