using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using Moq;
using Xunit;

namespace UnitTests.SagaStorage
{
    public class SagaPersisterUnitTests
    {
        private readonly FakeTestSagaDataDbSet _fakeDbSet;
        private readonly Mock<ISagaDbContext> _mockDbContext;
        private readonly Mock<IDbContextProvider> _mockDbContextProvider;
        private readonly SagaPersister _persister;

        public SagaPersisterUnitTests()
        {
            _fakeDbSet = new FakeTestSagaDataDbSet();
            _mockDbContext = new Mock<ISagaDbContext>();
            _mockDbContextProvider = new Mock<IDbContextProvider>();
            
            _mockDbContext.Setup(m => m.Set(It.IsAny<Type>())).Returns(_fakeDbSet);
            _mockDbContext.Setup(m => m.HasSet(It.IsAny<Type>())).Returns(true);
            _mockDbContextProvider.Setup(m => m.GetSagaDbContext()).Returns(_mockDbContext.Object);

            _persister = new SagaPersister(_mockDbContextProvider.Object);
        }

        [Fact]
        public void Complete_NullSaga_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Complete(null);

            // assert
            action.ShouldThrow<ArgumentNullException>()
                .WithMessage("*saga*");
        }

        [Fact]
        public void Get_EmptyId_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Get<TestSagaData>(Guid.Empty);

            // assert
            action.ShouldThrow<ArgumentException>()
                .WithMessage("*sagaId*");
        }

        [Fact]
        public void GetByProp_EmptyPropertyName_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Get<TestSagaData>(string.Empty, "something");

            // assert
            action.ShouldThrow<ArgumentNullException>()
                .WithMessage("*propertyName*");
        }

        [Fact]
        public void GetByProp_NullPropertyName_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Get<TestSagaData>(null, "something");

            // assert
            action.ShouldThrow<ArgumentNullException>()
                .WithMessage("*propertyName*");
        }

        [Fact]
        public void Save_NullSaga_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Save(null);

            // assert
            action.ShouldThrow<ArgumentNullException>()
                .WithMessage("*saga*");
        }

        [Fact]
        public void Update_NullSaga_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Update(null);

            // assert
            action.ShouldThrow<ArgumentNullException>()
                .WithMessage("*saga*");
        }
    }

    internal class FakeTestSagaDataDbSet : DbSet
    {
        private readonly List<TestSagaData> _set;

        public FakeTestSagaDataDbSet()
        {
            _set = new List<TestSagaData>();
        }

        public override object Add(object entity)
        {
            _set.Add((TestSagaData)entity);
            return entity;
        }
    }
}