// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.29

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public partial class Session
  {
    /// <summary>
    /// Occurs when <see cref="Session"/> is about to be disposed.
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to <see cref="Persist"/> changes.
    /// </summary>
    public event EventHandler Persisting;

    /// <summary>
    /// Occurs when <see cref="Session"/> persisted.
    /// </summary>
    public event EventHandler Persisted;

    /// <summary>
    /// Occurs when <see cref="Entity"/> created.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityCreated;

    /// <summary>
    /// Occurs when local <see cref="Key"/> created.
    /// </summary>
    public event EventHandler<KeyEventArgs> LocalKeyCreated;

    /// <summary>
    /// Occurs when field value is about to be read.
    /// </summary>
    public event EventHandler<FieldEventArgs> EntityFieldValueGetting;

    /// <summary>
    /// Occurs when field value was read successfully.
    /// </summary>
    public event EventHandler<FieldValueEventArgs> EntityFieldValueGet;

    /// <summary>
    /// Occurs when field value reading is completed.
    /// </summary>
    public event EventHandler<FieldValueGetCompletedEventArgs> EntityFieldValueGetCompleted;

    /// <summary>
    /// Occurs when is field value is about to be changed.
    /// </summary>
    public event EventHandler<FieldValueEventArgs> EntityFieldValueSetting;

    /// <summary>
    /// Occurs when field value was changed successfully.
    /// </summary>
    public event EventHandler<FieldValueSetEventArgs> EntityFieldValueSet;

    /// <summary>
    /// Occurs when field value changing is completed.
    /// </summary>
    public event EventHandler<FieldValueSetCompletedEventArgs> EntityFieldValueSetCompleted;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is about to remove.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityRemoving;

    /// <summary>
    /// Occurs when <see cref="Entity"/> removed.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityRemove;

    /// <summary>
    /// Occurs when <see cref="Entity"/> removing is completed.
    /// </summary>
    public event EventHandler<EntityRemoveCompletedEventArgs> EntityRemoveCompleted;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item is about to remove.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemRemoving;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removed.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemRemoved;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removing is completed.
    /// </summary>
    public event EventHandler<EntitySetItemActionCompletedEventArgs> EntitySetItemRemoveCompleted;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item is about to remove.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemAdding;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removed.
    /// </summary>
    public event EventHandler<EntitySetItemEventArgs> EntitySetItemAdd;

    /// <summary>
    /// Occurs when <see cref="EntitySetBase"/> item removing is completed.
    /// </summary>
    public event EventHandler<EntitySetItemActionCompletedEventArgs> EntitySetItemAddCompleted;


    /// <summary>
    /// The manager of <see cref="Entity"/>'s events.
    /// </summary>
    public EntityEventBroker EntityEventBroker { get; private set; }

    private void NotifyDisposing()
    {
      if (Disposing!=null)
        Disposing(this, EventArgs.Empty);
    }

    private void NotifyPersisting()
    {
      if (!IsSystemLogicOnly && Persisting!=null)
        Persisting(this, EventArgs.Empty);
    }

    private void NotifyPersisted()
    {
      if (!IsSystemLogicOnly && Persisted!=null)
        Persisted(this, EventArgs.Empty);
    }

    internal void NotifyLocalKeyCreated(Key key)
    {
      if (LocalKeyCreated != null)
        LocalKeyCreated(this, new KeyEventArgs(key));
    }

    internal void NotifyEntityCreated(Entity entity)
    {
      if (EntityCreated!=null)
        EntityCreated(this, new EntityEventArgs(entity));
    }

    internal void NotifyFieldValueGetting(Entity entity, FieldInfo field)
    {
      if (EntityFieldValueGetting!=null)
        EntityFieldValueGetting(this, new FieldEventArgs(entity, field));
    }

    internal void NotifyFieldValueGet(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueGet!=null)
        EntityFieldValueGet(this, new FieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueGetCompleted(Entity entity, FieldInfo field, object value, Exception exception)
    {
      if (EntityFieldValueGetCompleted != null)
        EntityFieldValueGetCompleted(this, new FieldValueGetCompletedEventArgs(entity, field, value, exception));
    }

    internal void NotifyFieldValueSetting(Entity entity, FieldInfo field, object value)
    {
      if (EntityFieldValueSetting!=null)
        EntityFieldValueSetting(this, new FieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueSet(Entity entity, FieldInfo field, object oldValue, object newValue)
    {
      if (EntityFieldValueSet!=null)
        EntityFieldValueSet(this, new FieldValueSetEventArgs(entity, field, oldValue, newValue));
    }

    internal void NotifyFieldValueSetCompleted(Entity entity, FieldInfo field, object oldValue, object newValue, Exception exception)
    {
      if (EntityFieldValueSetCompleted != null)
        EntityFieldValueSetCompleted(this, new FieldValueSetCompletedEventArgs(entity, field, oldValue, newValue, exception));
    }

    internal void NotifyEntityRemoving(Entity entity)
    {
      if (EntityRemoving!=null)
        EntityRemoving(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemove(Entity entity)
    {
      if (EntityRemove!=null)
        EntityRemove(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemoveCompleted(Entity entity, Exception exception)
    {
      if (EntityRemoveCompleted != null)
        EntityRemoveCompleted(this, new EntityRemoveCompletedEventArgs(entity, exception));
    }

    internal void NotifyEntitySetItemRemoving(EntitySetBase entitySet, Entity entity)
    {
      if (EntitySetItemRemoving != null)
        EntitySetItemRemoving(this, new EntitySetItemEventArgs(entity, entitySet));
    }

    internal void NotifyEntitySetItemRemoved(EntitySetBase entitySet, Entity entity)
    {
      if (EntitySetItemRemoved != null)
        EntitySetItemRemoved(this, new EntitySetItemEventArgs(entity, entitySet));
    }

    internal void NotifyEntitySetItemRemoveCompleted(EntitySetBase entitySet, Entity entity, Exception exception)
    {
      if (EntitySetItemRemoveCompleted != null)
        EntitySetItemRemoveCompleted(this, new EntitySetItemActionCompletedEventArgs(entity, entitySet, exception));
    }

    internal void NotifyEntitySetItemAdding(EntitySetBase entitySet, Entity entity)
    {
      if (EntitySetItemAdding != null)
        EntitySetItemAdding(this, new EntitySetItemEventArgs(entity, entitySet));
    }

    internal void NotifyEntitySetItemAdd(EntitySetBase entitySet, Entity entity)
    {
      if (EntitySetItemAdd != null)
        EntitySetItemAdd(this, new EntitySetItemEventArgs(entity, entitySet));
    }

    internal void NotifyEntitySetItemAddCompleted(EntitySetBase entitySet, Entity entity, Exception exception)
    {
      if (EntitySetItemAddCompleted != null)
        EntitySetItemAddCompleted(this, new EntitySetItemActionCompletedEventArgs(entity, entitySet, exception));
    }
  }
}