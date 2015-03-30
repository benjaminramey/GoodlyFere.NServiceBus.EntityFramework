#region Usings

using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus.Saga;

#endregion

namespace UnitTests
{
    internal class TestSagaDbContext : DbContext, ISagaDbContext
    {
        public TestSagaDbContext()
            : base("testdb")
        {
        }

        public DbSet<TestSagaData> TestSagas { get; set; }

        public DbSet SagaSet(Type sagaDataType)
        {
            if (sagaDataType == typeof(TestSagaData))
            {
                return TestSagas;
            }

            throw new ArgumentOutOfRangeException("No DbSets of type " + sagaDataType + " found.");
        }
    }

    public class TestSagaData : IContainSagaData
    {
        public string SomeProp1 { get; set; }
        public string SomeProp2 { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
        public Guid Id { get; set; }
    }
}