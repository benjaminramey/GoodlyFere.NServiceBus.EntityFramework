#region Usings

using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public class NServiceBusDbContextFactory : IDbContextFactory<NServiceBusDbContext>
    {
        public NServiceBusDbContext Create()
        {
            return new NServiceBusDbContext();
        }
    }
}