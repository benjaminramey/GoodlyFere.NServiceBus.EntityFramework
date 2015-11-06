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
using System.Data.Entity;
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
            TestSagaDataWithRowVersion sagaData = AddSagaWithRowVersion();
            var persisterRetrievedSagaData = _persister.Get<TestSagaDataWithRowVersion>(sagaData.Id);
            using (var dbc = new TestDbContext())
            {
                dbc.TestSagasWithRowVersion.Find(sagaData.Id).Should().NotBeNull();
            }

            // act
            _persister.Complete(persisterRetrievedSagaData);

            // assert
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagasWithRowVersion.Find(sagaData.Id).Should().BeNull();
            }
        }

        [Fact]
        public void Complete_DetachedEntity_Throws()
        {
            // arrange
            TestSagaData newSagaData = AddSaga();

            // act
            Action action = () => _persister.Complete(newSagaData);

            // assert
            action.ShouldThrow<DeletingDetachedEntityException>();
        }

        [Fact]
        public void Complete_NonExistentSaga_ThrowsDbUpdateConcurrencyEx()
        {
            // arrange
            var sagaData = AddSaga();
            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(sagaData.Id);
            using (var dbc = new TestDbContext())
            {
                var entry = dbc.Entry(sagaData);
                entry.State = EntityState.Deleted;
                dbc.SaveChanges();
            }

            // act
            Action action = () => _persister.Complete(persisterRetrievedSagaData);

            // assert
            action.ShouldThrow<DbUpdateConcurrencyException>();
        }

        [Fact]
        public void Complete_UpdatedSagaWithRowVersion_DeletesSaga()
        {
            // arrange
            TestSagaDataWithRowVersion sagaData = AddSagaWithRowVersion();
            var persisterRetrievedSagaData = _persister.Get<TestSagaDataWithRowVersion>(sagaData.Id);

            using (var dbc = new TestDbContext())
            {
                TestSagaDataWithRowVersion foundSaga = dbc.TestSagasWithRowVersion.Find(sagaData.Id);
                foundSaga.Should().NotBeNull();

                foundSaga.SomeProp1 = Guid.NewGuid().ToString(); // update prop
                dbc.SaveChanges();
            }

            // act
            _persister.Complete(persisterRetrievedSagaData);

            // assert
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagasWithRowVersion.Find(sagaData.Id).Should().BeNull();
            }
        }
        [Fact]
        public void Complete_UpdatedSagaWithoutARowVersion_DeletesSaga()
        {
            // arrange
            TestSagaData sagaData = AddSaga();
            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(sagaData.Id);

            using (var dbc = new TestDbContext())
            {
                TestSagaData foundSaga = dbc.TestSagaDatas.Find(sagaData.Id);
                foundSaga.Should().NotBeNull();

                foundSaga.SomeProp1 = Guid.NewGuid().ToString(); // update prop
                dbc.SaveChanges();
            }

            // act
            _persister.Complete(persisterRetrievedSagaData);

            // assert
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagaDatas.Find(sagaData.Id).Should().BeNull();
            }
        }

        [Fact]
        public void Get_Id_GetsSaga()
        {
            // arrange
            TestSagaDataWithRowVersion sagaData = AddSagaWithRowVersion();

            // act
            TestSagaDataWithRowVersion retrievedSaga = _persister.Get<TestSagaDataWithRowVersion>(sagaData.Id);

            // assert
            sagaData.ShouldBeEquivalentTo(retrievedSaga);
        }

        [Fact]
        public void GetByProp_GetsSaga()
        {
            // arrange
            var sagaData = new TestSagaDataWithRowVersion
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = Guid.NewGuid().ToString(),
                SomeProp2 = "somep prop 2"
            };
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagasWithRowVersion.Add(sagaData);
                dbContext.SaveChanges();
            }

            // act
            TestSagaDataWithRowVersion retrievedSaga = _persister.Get<TestSagaDataWithRowVersion>(
                "SomeProp1",
                sagaData.SomeProp1);

            // assert
            sagaData.ShouldBeEquivalentTo(retrievedSaga);
        }

        [Fact]
        public void GetByProp_NonExistentProp_Throws()
        {
            // arrange
            // act
            Action action = () => _persister.Get<TestSagaDataWithRowVersion>("non-existent property name", "bob");

            // assert
            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void GetByProp_NoResults_ReturnsNull()
        {
            // arrange
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagasWithRowVersion.RemoveRange(dbContext.TestSagasWithRowVersion);
                dbContext.SaveChanges();
            }

            // act
            TestSagaDataWithRowVersion result = _persister.Get<TestSagaDataWithRowVersion>("SomeProp1", "some value");

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public void Save_ShouldSaveSaga()
        {
            // arrange
            var sagaData = new TestSagaDataWithRowVersion
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
                TestSagaDataWithRowVersion fromDb = dbc.TestSagasWithRowVersion.Find(sagaData.Id);
                fromDb.ShouldBeEquivalentTo(sagaData);
            }
        }

        [Fact]
        public void Update_DbChangedEntityWithRowVersion_ShouldSaveSaga()
        {
            // arrange
            TestSagaDataWithRowVersion newSagaData = AddSagaWithRowVersion();
            string expectedSomeProp1 = Guid.NewGuid().ToString();
            string expectedSomeProp2 = Guid.NewGuid().ToString();

            var persisterRetrievedSagaData = _persister.Get<TestSagaDataWithRowVersion>(newSagaData.Id);

            // simulates another worker updating the saga after "this" saga retrieves it's saga
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagasWithRowVersion.Find(newSagaData.Id);
                fromDb.SomeProp1 = expectedSomeProp1;
                dbc.SaveChanges();
            }

            // act
            persisterRetrievedSagaData.SomeProp2 = expectedSomeProp2;
            _persister.Update(persisterRetrievedSagaData);

            // assert
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagasWithRowVersion.Find(newSagaData.Id);

                fromDb.Id.Should().Be(newSagaData.Id);
                fromDb.OriginalMessageId.Should().Be(newSagaData.OriginalMessageId);
                fromDb.Originator.Should().Be(newSagaData.Originator);

                fromDb.SomeProp1.Should().Be(expectedSomeProp1);
                fromDb.SomeProp2.Should().Be(expectedSomeProp2);
            }
        }

        [Fact]
        public void Update_DbChangedEntityWithoutARowVersion__ShouldSaveSaga()
        {
            // arrange
            TestSagaData newSagaData = AddSaga();
            string expectedSomeProp1 = Guid.NewGuid().ToString();
            string expectedSomeProp2 = Guid.NewGuid().ToString();

            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(newSagaData.Id);

            // simulates another worker updating the saga after "this" saga retrieves it's saga
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagaDatas.Find(newSagaData.Id);
                fromDb.SomeProp1 = expectedSomeProp1;
                dbc.SaveChanges();
            }

            // act
            persisterRetrievedSagaData.SomeProp2 = expectedSomeProp2;
            _persister.Update(persisterRetrievedSagaData);

            // assert
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagaDatas.Find(newSagaData.Id);

                fromDb.Id.Should().Be(newSagaData.Id);
                fromDb.OriginalMessageId.Should().Be(newSagaData.OriginalMessageId);
                fromDb.Originator.Should().Be(newSagaData.Originator);

                fromDb.SomeProp1.Should().Be(expectedSomeProp1);
                fromDb.SomeProp2.Should().Be(expectedSomeProp2);
            }
        }

        [Fact]
        public void Update_DetachedEntity_Throws()
        {
            // arrange
            TestSagaDataWithRowVersion newSagaData = AddSagaWithRowVersion();

            // act
            Action action = () => _persister.Update(newSagaData);

            // assert
            action.ShouldThrow<UpdatingDetachedEntityException>();
        }

        [Fact]
        public void Update_NonExistentSaga_DoesNothing()
        {
            // arrange
            var sagaData = AddSaga();
            var persisterRetrievedSagaData = _persister.Get<TestSagaData>(sagaData.Id);

            using (var dbc = new TestDbContext())
            {
                var entry = dbc.Entry(sagaData);
                entry.State = EntityState.Deleted;
                dbc.SaveChanges();
            }

            // act
            Action action = () => _persister.Update(persisterRetrievedSagaData);

            // assert
            action.ShouldNotThrow();
        }

        [Fact]
        public void Update_ShouldSaveSaga()
        {
            // arrange
            TestSagaDataWithRowVersion sagaData = AddSagaWithRowVersion();

            // act
            var persisterRetrievedSagaData = _persister.Get<TestSagaDataWithRowVersion>(sagaData.Id);
            persisterRetrievedSagaData.SomeProp1 = "some other value";
            _persister.Update(persisterRetrievedSagaData);

            // assert
            using (var dbc = new TestDbContext())
            {
                var fromDb = dbc.TestSagasWithRowVersion.Find(sagaData.Id);
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
                dbContext.TestSagaDatas.Add(sagaData);
                dbContext.SaveChanges();
            }
            return sagaData;
        }

        private static TestSagaDataWithRowVersion AddSagaWithRowVersion()
        {
            var sagaData = new TestSagaDataWithRowVersion
            {
                Id = Guid.NewGuid(),
                Originator = "originator yeah",
                OriginalMessageId = "original message id",
                SomeProp1 = "some prop 1",
                SomeProp2 = "somep prop 2"
            };
            using (var dbContext = new TestDbContext())
            {
                dbContext.TestSagasWithRowVersion.Add(sagaData);
                dbContext.SaveChanges();
            }
            return sagaData;
        }
    }
}