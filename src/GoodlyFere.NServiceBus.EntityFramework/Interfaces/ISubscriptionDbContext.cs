#region Usings

using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Interfaces
{
    public interface ISubscriptionDbContext : INServiceBusDbContext
    {
        DbSet<SubscriptionEntity> Subscriptions { get; set; }
    }
}