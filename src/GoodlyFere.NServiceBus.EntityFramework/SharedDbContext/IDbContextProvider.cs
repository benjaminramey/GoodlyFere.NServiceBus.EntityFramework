using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    public interface IDbContextProvider
    {
        ISagaDbContext GetSagaDbContext();
    }
}