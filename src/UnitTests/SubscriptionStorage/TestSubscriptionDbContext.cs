#region Usings

using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using NServiceBus.Saga;

#endregion

namespace UnitTests.SubscriptionStorage
{
    internal class TestSubscriptionDbContext : DbContext, ISubscriptionDbContext
    {
        public TestSubscriptionDbContext()
            : base("testdb")
        {
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    }
}