#region Usings

using System;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Interfaces
{
    public interface INServiceBusDbContextFactory
    {
        ISagaDbContext CreateSagaDbContext();

        ITimeoutDbContext CreateTimeoutDbContext();

        ISubscriptionDbContext CreateSubscriptionDbContext();
    }
}