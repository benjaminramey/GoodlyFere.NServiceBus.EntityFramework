using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using NServiceBus.Saga;

namespace AcceptanceTests
{
    public class TestDbContext : NServiceBusDbContext
    {
        private readonly Dictionary<Type, DbContext> _dbContexts;

        public TestDbContext()
            : base("TestDbContext")
        {
            _dbContexts = new Dictionary<Type, DbContext>();
            Database.SetInitializer(new CreateDatabaseIfNotExists<TestDbContext>());
        }

        public override DbSet Set(Type sagaDataType)
        {
            DbContext dbContext;
            Type realType = sagaDataType;
            
            if (_dbContexts.ContainsKey(realType))
            {
                dbContext = _dbContexts[realType];
            }
            else
            {
                Type dbContextType = typeof(TestGenericDbContext<>).MakeGenericType(realType);
                dbContext = (DbContext)Activator.CreateInstance(dbContextType, "TestDbContext");
                _dbContexts.Add(realType, dbContext);
            }

            return dbContext.Set(realType);
        }

        public override int SaveChanges()
        {
            foreach (var dbContext in _dbContexts.Values)
            {
                dbContext.SaveChanges();
            }

            return base.SaveChanges();
        }
    }

    public class TestGenericDbContext<TSagaDataType> : DbContext
        where TSagaDataType : class, IContainSagaData
    {
        public TestGenericDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<TSagaDataType> SagaData { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var type = typeof(TSagaDataType);
            string tableName = type.Name + "_" + (type.Namespace ?? "no.namespace").Replace(".", "_");

            modelBuilder.Entity<TSagaDataType>()
                .ToTable(tableName);

            base.OnModelCreating(modelBuilder);
        }
    }
}