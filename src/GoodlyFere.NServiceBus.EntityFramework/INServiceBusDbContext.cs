#region Usings

using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public interface INServiceBusDbContext
    {
        Database Database { get; }
        DbSet<TimeoutDataEntity> Timeouts { get; set; }

        int SaveChanges();
    }
}