#region Usings

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using Moq;
using NServiceBus.IdGeneration;
using Xunit;

#endregion

namespace UnitTests.SagaStorage
{
    public class SagaPersisterTests
    {
        private readonly Mock<INServiceBusDbContextFactory> _mockFactory;
        private readonly SagaPersister _persister;

        public SagaPersisterTests()
        {
            _mockFactory = new Mock<INServiceBusDbContextFactory>();
            _mockFactory.Setup(m => m.CreateSagaDbContext()).Returns(new TestSagaDbContext());

            _persister = new SagaPersister(_mockFactory.Object);
        }

        [Fact]
        public void Save_NullSaga_Throws()
        {
            _persister.Invoking(p => p.Save(null)).ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Save_ShouldCreateSagaDbContext()
        {
            var sagaData = new TestSagaData();

            _persister.Save(sagaData);

            _mockFactory.Verify(m => m.CreateSagaDbContext(), Times.Once());
        }

        [Fact]
        public void Save_ShouldSaveSaga()
        {
            var sagaData = new TestSagaData
            {
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = "some prop 1",
                SomeProp2 = "somep prop 2"
            };

            _persister.Save(sagaData);

            sagaData.Id.Should().NotBeEmpty();
            var fromDb = (new TestSagaDbContext()).TestSagas.Find(sagaData.Id);

            fromDb.ShouldBeEquivalentTo(sagaData);
            fromDb.Id.Should().Be(sagaData.Id);
            fromDb.OriginalMessageId.Should().Be(sagaData.OriginalMessageId);
            fromDb.Originator.Should().Be(sagaData.Originator);
            fromDb.SomeProp1.Should().Be(sagaData.SomeProp1);
            fromDb.SomeProp2.Should().Be(sagaData.SomeProp2);
        }

        [Fact]
        public void Update_NullSaga_Throws()
        {
            _persister.Invoking(p => p.Update(null)).ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Update_ShouldSaveSaga()
        {
            var sagaData = new TestSagaData
            {
                Id= Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = "some prop 1",
                SomeProp2 = "somep prop 2"
            };
            var dbContext = new TestSagaDbContext();

            dbContext.TestSagas.Add(sagaData);
            dbContext.SaveChanges();

            sagaData.SomeProp1 = "some other value";
            _persister.Update(sagaData);

            var fromDb = (new TestSagaDbContext()).TestSagas.Find(sagaData.Id);

            fromDb.ShouldBeEquivalentTo(sagaData);
            fromDb.Id.Should().Be(sagaData.Id);
            fromDb.OriginalMessageId.Should().Be(sagaData.OriginalMessageId);
            fromDb.Originator.Should().Be(sagaData.Originator);
            fromDb.SomeProp1.Should().Be(sagaData.SomeProp1);
            fromDb.SomeProp2.Should().Be(sagaData.SomeProp2);
        }

        [Fact]
        public void Get_EmptyId_Throws()
        {
            _persister.Invoking(p => p.Get<TestSagaData>(Guid.Empty)).ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Get_Id_GetsSaga()
        {
            var sagaData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = "some prop 1",
                SomeProp2 = "somep prop 2"
            };
            var dbContext = new TestSagaDbContext();

            dbContext.TestSagas.Add(sagaData);
            dbContext.SaveChanges();

            var retrievedSaga = _persister.Get<TestSagaData>(sagaData.Id);

            sagaData.ShouldBeEquivalentTo(retrievedSaga);
            sagaData.Id.Should().Be(retrievedSaga.Id);
            sagaData.OriginalMessageId.Should().Be(retrievedSaga.OriginalMessageId);
            sagaData.Originator.Should().Be(retrievedSaga.Originator);
            sagaData.SomeProp1.Should().Be(retrievedSaga.SomeProp1);
            sagaData.SomeProp2.Should().Be(retrievedSaga.SomeProp2);
        }

        [Fact]
        public void GetByProp_GetsSaga()
        {
            var sagaData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = Guid.NewGuid().ToString(),
                SomeProp2 = "somep prop 2"
            };
            var dbContext = new TestSagaDbContext();

            dbContext.TestSagas.Add(sagaData);
            dbContext.SaveChanges();

            var retrievedSaga = _persister.Get<TestSagaData>("SomeProp1", sagaData.SomeProp1);

            sagaData.ShouldBeEquivalentTo(retrievedSaga);
            sagaData.Id.Should().Be(retrievedSaga.Id);
            sagaData.OriginalMessageId.Should().Be(retrievedSaga.OriginalMessageId);
            sagaData.Originator.Should().Be(retrievedSaga.Originator);
            sagaData.SomeProp1.Should().Be(retrievedSaga.SomeProp1);
            sagaData.SomeProp2.Should().Be(retrievedSaga.SomeProp2);
        }
    }
}