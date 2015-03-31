#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeoutPersisterTests.cs">
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
// <created>03/31/2015 9:38 AM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;
using Moq;
using NServiceBus;
using NServiceBus.Timeout.Core;
using Xunit;

#endregion

namespace UnitTests.TimeoutStorage
{
    public class TimeoutPersisterTests
    {
        private readonly TestDbContext _dbContext;
        private readonly Mock<INServiceBusDbContextFactory> _mockFactory;
        private readonly TimeoutPersister _persister;

        public TimeoutPersisterTests()
        {
            _dbContext = new TestDbContext();

            _mockFactory = new Mock<INServiceBusDbContextFactory>();
            _mockFactory.Setup(m => m.CreateTimeoutDbContext()).Returns(new TestDbContext());

            _persister = new TimeoutPersister(_mockFactory.Object);
            _persister.EndpointName = "queue@machine";

            _dbContext.Timeouts.RemoveRange(_dbContext.Timeouts);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void Add_AddsTimeout()
        {
            var timeout = new TimeoutData
            {
                Destination = new Address("queue", "machine"),
                OwningTimeoutManager = "owner",
                Headers = null,
                Id = Guid.NewGuid().ToString(),
                SagaId = Guid.NewGuid(),
                State = null,
                Time = DateTime.UtcNow
            };

            _persister.Add(timeout);

            var actual = _dbContext.Timeouts.FirstOrDefault(t => t.SagaId == timeout.SagaId);

            actual.Should().NotBeNull();
        }

        [Fact]
        public void Add_NullSaga_Throws()
        {
            _persister.Invoking(p => p.Add(null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void GetNextChunk_GetsNextTimeouts()
        {
            AddTimeouts();

            DateTime nextTimeToRun;
            var result = _persister.GetNextChunk(DateTime.UtcNow.AddDays(-5), out nextTimeToRun);

            result.Count().Should().Be(5);
        }

        [Fact]
        public void GetNextChunk_NextTimeSetTo10Minutes()
        {
            DateTime nextTimeToRun;
            var result = _persister.GetNextChunk(DateTime.UtcNow.AddDays(-5), out nextTimeToRun);

            nextTimeToRun.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), 1000);
        }

        [Fact]
        public void RemoveTimeoutBy_EmptySagaId_Throws()
        {
            _persister.Invoking(p => p.RemoveTimeoutBy(Guid.Empty))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void RemoveTimeoutBy_RemovesAllTimeoutsForSaga()
        {
            AddTimeouts();
            var timeout = _dbContext.Timeouts.First();
            var another = Build(DateTime.UtcNow);
            var yetAnother = Build(DateTime.UtcNow);
            another.SagaId = timeout.SagaId;
            yetAnother.SagaId = timeout.SagaId;
            _dbContext.Timeouts.Add(another);
            _dbContext.Timeouts.Add(yetAnother);
            _dbContext.SaveChanges();

            _persister.RemoveTimeoutBy(timeout.SagaId);

            _dbContext.Timeouts.Count(t => t.SagaId == timeout.SagaId).Should().Be(0);
        }

        [Fact]
        public void TryRemove_Removes()
        {
            AddTimeouts();
            var timeout = _dbContext.Timeouts.First();

            TimeoutData data;
            var result = _persister.TryRemove(timeout.Id.ToString(), out data);

            result.Should().BeTrue();

            var dbc = new TestDbContext();
            dbc.Timeouts.Find(timeout.Id).Should().BeNull();
        }

        [Fact]
        public void TryRemove_NonExistentId_ReturnsFalseOutIsNull()
        {
            TimeoutData data;
            var result = _persister.TryRemove(Guid.NewGuid().ToString(), out data);

            result.Should().BeFalse();
            data.Should().BeNull();
        }

        [Fact]
        public void TryRemove_PopulatesOutData()
        {
            AddTimeouts();
            var timeout = _dbContext.Timeouts.First();

            TimeoutData data;
            var result = _persister.TryRemove(timeout.Id.ToString(), out data);

            data.Id.Should().Be(timeout.Id.ToString());
        }

        private void AddTimeouts()
        {
            _dbContext.Timeouts.AddRange(
                new List<TimeoutDataEntity>
                {
                    Build(DateTime.UtcNow.AddDays(-5)),
                    Build(DateTime.UtcNow.AddDays(-4)),
                    Build(DateTime.UtcNow.AddDays(-3)),
                    Build(DateTime.UtcNow.AddDays(-2)),
                    Build(DateTime.UtcNow.AddDays(-1)),
                    Build(DateTime.UtcNow),
                    Build(DateTime.UtcNow.AddDays(1)),
                    Build(DateTime.UtcNow.AddDays(2)),
                    Build(DateTime.UtcNow.AddDays(3)),
                    Build(DateTime.UtcNow.AddDays(4)),
                    Build(DateTime.UtcNow.AddDays(5)),
                });
            _dbContext.SaveChanges();
        }

        private TimeoutDataEntity Build(DateTime time)
        {
            return new TimeoutDataEntity
            {
                Destination = new Address("queue", "machine2").ToString(),
                Endpoint = new Address("queue", "machine").ToString(),
                Headers = null,
                Id = Guid.NewGuid(),
                SagaId = Guid.NewGuid(),
                State = null,
                Time = time
            };
        }
    }
}