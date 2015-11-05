using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;

namespace AcceptanceTests
{
    public class TestDbContextFactory : INServiceBusDbContextFactory
    {
        public ISagaDbContext CreateSagaDbContext()
        {
            return new TestDbContext();
        }

        public ISubscriptionDbContext CreateSubscriptionDbContext()
        {
            return new TestDbContext();
        }

        public ITimeoutDbContext CreateTimeoutDbContext()
        {
            return new TestDbContext();
        }
    }
}