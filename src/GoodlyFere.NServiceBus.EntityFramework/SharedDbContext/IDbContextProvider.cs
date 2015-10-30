using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    public interface IDbContextProvider
    {
        ISagaDbContext GetSagaDbContext();

        ISubscriptionDbContext GetSubscriptionDbContext();

        ITimeoutDbContext GetTimeoutDbContext();
    }
}