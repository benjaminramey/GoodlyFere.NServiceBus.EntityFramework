#region Usings

using System;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using Moq;
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
            _mockFactory.Setup(m => m.CreateSagaDbContext()).Returns(new TestDbContext());

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
            var fromDb = (new TestDbContext()).TestSagas.Find(sagaData.Id);

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
            var sagaData = AddSaga();

            sagaData.SomeProp1 = "some other value";
            _persister.Update(sagaData);

            var fromDb = (new TestDbContext()).TestSagas.Find(sagaData.Id);

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
            var sagaData = AddSaga();

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
            var dbContext = new TestDbContext();

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

        [Fact]
        public void GetByProp_NullPropName_Throws()
        {
            _persister.Invoking(p => p.Get<TestSagaData>(null, "bob"))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void GetByProp_EmptyPropName_Throws()
        {
            _persister.Invoking(p => p.Get<TestSagaData>(string.Empty, "bob"))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void GetByProp_NonExistentProp_Throws()
        {
            _persister.Invoking(p => p.Get<TestSagaData>("non-existent property name", "bob"))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void GetByProp_NoResults_ReturnsNull()
        {
            // make sure there are no testsagadata items in db
            var dbContext = new TestDbContext();
            dbContext.Database.ExecuteSqlCommand("delete from testsagadatas");

            var result = _persister.Get<TestSagaData>("SomeProp1", "some value");

            result.Should().BeNull();
        }

        // complete, delete saga
        [Fact]
        public void Complete_DeletesSaga()
        {
            var sagaData = AddSaga();

            _persister.Complete(sagaData);

            var dbContext = new TestDbContext();
            dbContext.TestSagas.Find(sagaData.Id).Should().BeNull();
        }

        [Fact]
        public void Complete_NullSaga_Throws()
        {
            _persister.Invoking(p => p.Complete(null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Complete_NonExistentSaga_DoesNothing()
        {
            var sagaData = new TestSagaData();

            _persister.Invoking(p => p.Complete(sagaData))
                .ShouldNotThrow();
        }

        private static TestSagaData AddSaga()
        {
            var sagaData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = "some prop 1",
                SomeProp2 = "somep prop 2"
            };
            var dbContext = new TestDbContext();

            dbContext.TestSagas.Add(sagaData);
            dbContext.SaveChanges();
            return sagaData;
        }
    }
}