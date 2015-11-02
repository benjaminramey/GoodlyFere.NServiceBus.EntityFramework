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
            : base("NServiceBusDbContext")
        {
        }

        public virtual DbSet SagaSet(Type sagaDataType)
        {
            throw new NotImplementedException();
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
        public DbSet<TimeoutDataEntity> Timeouts { get; set; }
    }
}