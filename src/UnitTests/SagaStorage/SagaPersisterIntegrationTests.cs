#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SagaPersisterTests.cs">
//  Copyright 2015 Benjamin S. Ramey
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </copyright>
// <created>03/26/2015 3:45 PM</created>
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Data.Entity.Infrastructure;
using FluentAssertions;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Exceptions;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using Moq;
using Xunit;

#endregion

namespace UnitTests.SagaStorage
{
    public class SagaPersisterIntegrationTests : IDisposable
    {
        private readonly TestDbContext _dbContext;
        private readonly Mock<IDbContextProvider> _mockDbContextProvider;
        private readonly SagaPersister _persister;

        public SagaPersisterIntegrationTests()
        {
            _dbContext = new TestDbContext();

            _mockDbContextProvider = new Mock<IDbContextProvider>();
            _mockDbContextProvider.Setup(m => m.GetSagaDbContext()).Returns(_dbContext);

            _persister = new SagaPersister(_mockDbContextProvider.Object);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        // complete, delete saga
        [Fact]
        public void Complete_ExistingSaga_DeletesSaga()
        {
            // arrange
            TestSagaData sagaData = AddSaga();
            using (var dbc = new TestDbContext())
            {
                dbc.TestSagas.Find(sagaData.Id).Should().NotBeNull();
            }

            // act
            _persister.Complete(sagaData);

            // assert
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagas.Find(sagaData.Id).Should().BeNull();
            }
        }
        [Fact]
        public void Complete_UpdatedSaga_DeletesSaga()
        {
            // arrange
            TestSagaData sagaData = AddSaga();
            using (var dbc = new TestDbContext())
            {
                TestSagaData foundSaga = dbc.TestSagas.Find(sagaData.Id);
                foundSaga.Should().NotBeNull();

                foundSaga.SomeProp1 = Guid.NewGuid().ToString(); // update prop
                dbc.SaveChanges();
            }

            // act
            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(sagaData.Id);
            _persister.Complete(persisterRetrievedSagaData);

            // assert
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagas.Find(sagaData.Id).Should().BeNull();
            }
        }

        [Fact]
        public void Complete_NonExistentSaga_ThrowsDbUpdateConcurrencyEx()
        {
            // arrange
            var sagaData = new TestSagaData();

            // act
            Action action = () => _persister.Complete(sagaData);

            // assert
            action.ShouldThrow<DbUpdateConcurrencyException>();
        }

        [Fact]
        public void Get_Id_GetsSaga()
        {
            // arrange
            TestSagaData sagaData = AddSaga();

            // act
            TestSagaData retrievedSaga = _persister.Get<TestSagaData>(sagaData.Id);

            // assert
            sagaData.ShouldBeEquivalentTo(retrievedSaga);
        }

        [Fact]
        public void GetByProp_GetsSaga()
        {
            // arrange
            var sagaData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = Guid.NewGuid().ToString(),
                SomeProp2 = "somep prop 2"
            };
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagas.Add(sagaData);
                dbContext.SaveChanges();
            }

            // act
            TestSagaData retrievedSaga = _persister.Get<TestSagaData>("SomeProp1", sagaData.SomeProp1);

            // assert
            sagaData.ShouldBeEquivalentTo(retrievedSaga);
        }

        [Fact]
        public void GetByProp_NonExistentProp_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Get<TestSagaData>("non-existent property name", "bob");

            // assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void GetByProp_NoResults_ReturnsNull()
        {
            // arrange
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagas.RemoveRange(dbContext.TestSagas);
                dbContext.SaveChanges();
            }

            // act
            TestSagaData result = _persister.Get<TestSagaData>("SomeProp1", "some value");

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public void Save_ShouldSaveSaga()
        {
            // arrange
            var sagaData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = "some prop 1",
                SomeProp2 = "somep prop 2"
            };

            // act
            _persister.Save(sagaData);

            // assert
            using (var dbc = new TestDbContext())
            {
                TestSagaData fromDb = dbc.TestSagas.Find(sagaData.Id);
                fromDb.ShouldBeEquivalentTo(sagaData);
            }
        }

        [Fact]
        public void Update_NonExistentSaga_Throws()
        {
            // arrange
            var saga = new TestSagaData
            {
                Id = Guid.NewGuid()
            };

            // act
            Action action = () => _persister.Update(saga);

            // assert
            action.ShouldThrow<Exception>();
        }

        [Fact]
        public void Update_DetachedEntity_Throws()
        {
            // arrange
            TestSagaData newSagaData = AddSaga();

            // act
            Action action = () => _persister.Update(newSagaData);

            // assert
            action.ShouldThrow<UpdatingDetachedEntityException>();
        }

        [Fact]
        public void Update_DbChangedEntity_ShouldSaveSaga()
        {
            // arrange
            TestSagaData newSagaData = AddSaga();
            string expectedSomeProp1 = Guid.NewGuid().ToString();
            string expectedSomeProp2 = Guid.NewGuid().ToString();
            
            // simulates another worker updating the saga after "this" saga retrieves it's saga
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagas.Find(newSagaData.Id);
                fromDb.SomeProp1 = expectedSomeProp1;
                dbc.SaveChanges();
            }

            // act
            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(newSagaData.Id);
            persisterRetrievedSagaData.SomeProp2 = expectedSomeProp2;
            _persister.Update(persisterRetrievedSagaData);

            // assert
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagas.Find(newSagaData.Id);
                
                fromDb.Id.Should().Be(newSagaData.Id);
                fromDb.OriginalMessageId.Should().Be(newSagaData.OriginalMessageId);
                fromDb.Originator.Should().Be(newSagaData.Originator);

                fromDb.SomeProp1.Should().Be(expectedSomeProp1);
                fromDb.SomeProp2.Should().Be(expectedSomeProp2);
            }
        }

        [Fact]
        public void Update_ShouldSaveSaga()
        {
            // arrange
            TestSagaData sagaData = AddSaga();

            // act
            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(sagaData.Id);
            persisterRetrievedSagaData.SomeProp1 = "some other value";
            _persister.Update(persisterRetrievedSagaData);

            // assert
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagas.Find(sagaData.Id);
                fromDb.ShouldBeEquivalentTo(persisterRetrievedSagaData);
            }
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
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagas.Add(sagaData);
                dbContext.SaveChanges();
            }
            return sagaData;
        }
    }
}