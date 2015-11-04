using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    public abstract class NServiceBusDbContext : DbContext, ISubscriptionDbContext, ITimeoutDbContext, ISagaDbContext
    {
        protected NServiceBusDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public virtual bool HasSet(Type entityType)
        {
            return Set(entityType) != null;
        }

        public virtual DbSet<SubscriptionEntity> Subscriptions { get; set; }
        public virtual DbSet<TimeoutDataEntity> Timeouts { get; set; }
    }
}