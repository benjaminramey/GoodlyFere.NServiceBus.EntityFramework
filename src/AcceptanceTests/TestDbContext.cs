using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;
using NServiceBus.AcceptanceTests.Sagas;

namespace AcceptanceTests
{
    public class TestDbContext : DbContext, ISubscriptionDbContext, ITimeoutDbContext, ISagaDbContext
    {
        public TestDbContext()
            : base("TestDbContext")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<TestDbContext>());
        }

        public DbSet<TestSagaData> TestSagas { get; set; }

        public DbSet<Issue_1819.Endpoint.Saga1.Saga1Data> Saga1Data1 { get; set; }

        public DbSet<When_started_by_event_from_another_saga.SagaThatPublishesAnEvent.Saga1.Saga1Data> Saga1Data2 { get;
            set; }

        public DbSet<Issue_2044.ReceiverWithSaga.Saga1.Saga1Data> Saga1Data3 { get; set; }
        public DbSet<When_sagas_cant_be_found.ReceiverWithOrderedSagas.Saga1.Saga1Data> Saga1Data4 { get; set; }
        public DbSet<When_sagas_cant_be_found.ReceiverWithSagas.Saga1.Saga1Data> Saga1Data5 { get; set; }
        public DbSet<When_sending_from_a_saga_handle.Endpoint.Saga1Data> Saga1Data6 { get; set; }
        public DbSet<When_sending_from_a_saga_timeout.Endpoint.Saga1.Saga1Data> Saga1Data7 { get; set; }

        public DbSet<When_sagas_cant_be_found.ReceiverWithOrderedSagas.Saga2.Saga2Data> Saga1Data8 { get; set; }
        public DbSet<When_sagas_cant_be_found.ReceiverWithSagas.Saga2.Saga2Data> Saga1Data9 { get; set; }
        public DbSet<When_sending_from_a_saga_handle.Endpoint.Saga2.Saga2Data> Saga1Data10 { get; set; }
        public DbSet<When_sending_from_a_saga_timeout.Endpoint.Saga2.Saga2Data> Saga1Data11 { get; set; }

        public DbSet<When_started_by_event_from_another_saga.SagaThatIsStartedByTheEvent.Saga2.Saga2Data> Saga1Data12 {
            get; set; }

        public DbSet SagaSet(Type sagaDataType)
        {
            Type dbContextType = typeof(TestDbContext);
            List<PropertyInfo> sagaProps = dbContextType.GetProperties().Where(p => p.Name.Contains("Saga")).ToList();

            PropertyInfo sagProperty =
                sagaProps.Single(s => s.PropertyType.GenericTypeArguments.Any(t => t == sagaDataType));

            if (sagProperty == null)
            {
                throw new NotImplementedException(
                    string.Format("Can't find property for saga type: {0}", sagaDataType.FullName));
            }

            return (DbSet)sagProperty.GetValue(this);
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }

        public DbSet<TimeoutDataEntity> Timeouts { get; set; }
    }
}