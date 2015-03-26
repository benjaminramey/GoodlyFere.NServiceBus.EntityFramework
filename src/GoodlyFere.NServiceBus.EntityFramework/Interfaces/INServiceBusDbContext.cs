#region Usings

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Interfaces
{
    public interface INServiceBusDbContext : IDisposable
    {
        Database Database { get; }

        int SaveChanges();

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbSet Set(Type entityType);

        DbEntityEntry Entry(object entity);
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}