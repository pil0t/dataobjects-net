// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides data for <see cref="Session"/> transaction-related events.
  /// </summary>
  public sealed class TransactionEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the transaction.
    /// </summary>
    public Transaction Transaction { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public TransactionEventArgs(Transaction transaction)
    {
      Transaction = transaction;
    }
  }
}