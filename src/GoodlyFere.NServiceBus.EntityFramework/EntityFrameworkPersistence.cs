#region Usings

using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;
using NServiceBus.Features;
using NServiceBus.Persistence;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    /// <summary>
    ///     EntityFramework persistence for NServiceBus.
    /// </summary>
    public class EntityFrameworkPersistence : PersistenceDefinition
    {
        /// <summary>
        ///     Defines the capabilities of the storage
        /// </summary>
        public EntityFrameworkPersistence()
        {
            Supports<StorageType.Timeouts>(s => s.EnableFeatureByDefault<EntityFrameworkTimeoutStorage>());
            Supports<StorageType.Subscriptions>(s => s.EnableFeatureByDefault<EntityFrameworkSubscriptionStorage>());
            Supports<StorageType.Sagas>(s => s.EnableFeatureByDefault<EntityFrameworkSagaStorage>());
        }
    }
}