#region Usings

using System;
using System.Data.Entity;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Interfaces
{
    public interface ISagaDbContext : INServiceBusDbContext
    {
        DbSet SagaSet(Type sagaDataType);
    }
}