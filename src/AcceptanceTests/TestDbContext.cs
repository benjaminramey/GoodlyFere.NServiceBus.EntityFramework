using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;
using NServiceBus.AcceptanceTests.Sagas;
using NServiceBus.Saga;

namespace AcceptanceTests
{
    public class TestDbContext : DbContext, ISubscriptionDbContext, ITimeoutDbContext, ISagaDbContext
    {
        private readonly Dictionary<Type, DbContext> DbContexts;

        public TestDbContext()
            : base("TestDbContext")
        {
            DbContexts = new Dictionary<Type, DbContext>();
            Database.SetInitializer(new DropCreateDatabaseAlways<TestDbContext>());
        }

        public DbSet SagaSet(Type sagaDataType)
        {
            DbContext dbContext;
            Type dbContextType;

            if (DbContexts.ContainsKey(sagaDataType))
            {
                dbContext = DbContexts[sagaDataType];
                dbContextType = dbContext.GetType();
            }
            else
            {
                dbContextType = typeof(TestGenericDbContext<>).MakeGenericType(sagaDataType);
                dbContext = (DbContext)Activator.CreateInstance(dbContextType);
                DbContexts.Add(sagaDataType, dbContext);
            }

            PropertyInfo prop = dbContextType.GetProperty("SagaData");
            return (DbSet)prop.GetValue(dbContext);
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }

        public DbSet<TimeoutDataEntity> Timeouts { get; set; }
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