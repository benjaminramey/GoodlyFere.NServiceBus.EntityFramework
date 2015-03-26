#region Usings

using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Interfaces
{
    public interface ITimeoutDbContext : INServiceBusDbContext
    {
        DbSet<TimeoutDataEntity> Timeouts { get; set; }
    }
}