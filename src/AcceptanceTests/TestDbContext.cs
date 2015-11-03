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

            if (_dbContexts.ContainsKey(sagaDataType))
            {
                dbContext = _dbContexts[sagaDataType];
            }
            else
            {
                Type dbContextType = typeof(TestGenericDbContext<>).MakeGenericType(sagaDataType);
                dbContext = (DbContext)Activator.CreateInstance(dbContextType, "TestDbContext");
                _dbContexts.Add(sagaDataType, dbContext);
            }

            return dbContext.Set(sagaDataType);
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
            string tableName = typeof(TSagaDataType).Name + "_" + Guid.NewGuid();

            modelBuilder.Entity<TSagaDataType>()
                .ToTable(tableName);

            base.OnModelCreating(modelBuilder);
        }
    }
}