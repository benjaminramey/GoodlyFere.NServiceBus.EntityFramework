#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubscriptionPersisterTests.cs">
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
// <created>03/30/2015 2:46 PM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using Moq;
using NServiceBus;
using NServiceBus.Unicast.Subscriptions;
using Xunit;

#endregion

namespace UnitTests.SubscriptionStorage
{
    public class SubscriptionPersisterTests : IDisposable
    {
        private readonly TestDbContext _dbContext;
        private readonly Mock<INServiceBusDbContextFactory> _mockFactory;
        private readonly SubscriptionPersister _persister;

        public SubscriptionPersisterTests()
        {
            _dbContext = new TestDbContext();

            _mockFactory = new Mock<INServiceBusDbContextFactory>();
            _mockFactory.Setup(m => m.CreateSubscriptionDbContext()).Returns(new TestDbContext());

            _persister = new SubscriptionPersister(_mockFactory.Object);

            _dbContext.Subscriptions.RemoveRange(_dbContext.Subscriptions);
            _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }

        //subscribe, null address, throws
        [Fact]
        public void Subscribe_NullClient_Throws()
        {
            _persister.Invoking(p => p.Subscribe(null, new List<MessageType>()))
                .ShouldThrow<ArgumentNullException>();
        }

        //subscribe, null message types, throws
        [Fact]
        public void Subscribe_NullMessageTypes_Throws()
        {
            _persister.Invoking(p => p.Subscribe(new Address("queue", "machine"), null))
                .ShouldThrow<ArgumentNullException>();
        }

        //subscribe, empty messages, saves nothing
        [Fact]
        public void Subscribe_EmptyMessageTypes_SavesNothing()
        {
            int expectedCount = _dbContext.Subscriptions.Count();

            _persister.Subscribe(
                new Address("queue", "machine"),
                new List<MessageType>());

            int actualCount = _dbContext.Subscriptions.ToList().Count;

            actualCount.Should().Be(expectedCount);
        }

        // subscribe, duplicate message types, saves unique records
        [Fact]
        public void Subscribe_DuplicateMessageTypes_SavesUniqueRecords()
        {
            _persister.Subscribe(
                new Address("queue", "machine"),
                new List<MessageType>
                {
                    new MessageType(typeof(TestMessage)),
                    new MessageType(typeof(TestMessage)),
                    new MessageType(typeof(TestMessage)),
                    new MessageType(typeof(TestMessage2)),
                });

            var actualEntities = _dbContext.Subscriptions.ToList();

            actualEntities.Count.Should().Be(2);
            actualEntities.Select(e => e.MessageType).Distinct().Count().Should().Be(2);
        }

        //unsubscribe, null client, throws
        [Fact]
        public void Unsubscribe_NullClient_Throws()
        {
            _persister.Invoking(p => p.Unsubscribe(null, new List<MessageType>()))
                .ShouldThrow<ArgumentNullException>();
        }

        // unsubscribe, null messagetypes throws
        [Fact]
        public void Unsubscribe_NullMessageTypes_Throws()
        {
            _persister.Invoking(p => p.Unsubscribe(new Address("queue", "machine"), null))
                .ShouldThrow<ArgumentNullException>();
        }

        // unsubscribe, removes all subscriptions
        [Fact]
        public void Unsubscribe_RemovesAllSubscriptions()
        {
            AddSubscriptions();

            string messageType = new MessageType(typeof(TestMessage)).ToString();
            _dbContext.Subscriptions.Count(
                s => s.SubscriberEndpoint == "queue@machine"
                     && s.MessageType == messageType)
                .Should().BeGreaterThan(0);

            _persister.Unsubscribe(
                new Address("queue", "machine"),
                new List<MessageType>
                {
                    new MessageType(typeof(TestMessage))
                });

            _dbContext.Subscriptions.Count(
                s => s.SubscriberEndpoint == "queue@machine"
                     && s.MessageType == messageType)
                .Should().Be(0);
        }

        [Fact]
        public void GetSubscriberAddressesForMessage_GetsUniqueAddressesForMessages()
        {
            AddSubscriptions();
            string mtString = new MessageType(typeof(TestMessage)).ToString();
            string mtString2 = new MessageType(typeof(TestMessage2)).ToString();
            int expectedCount = _dbContext.Subscriptions
                .Where(
                    s =>
                        s.MessageType == mtString
                        || s.MessageType == mtString2)
                .Select(s => s.SubscriberEndpoint)
                .Distinct()
                .Count();

            var result = _persister.GetSubscriberAddressesForMessage(
                new List<MessageType>
                {
                    new MessageType(typeof(TestMessage)),
                    new MessageType(typeof(TestMessage2))
                });

            result.Count().Should().BeGreaterThan(0);
            result.Count().Should().Be(expectedCount);
            result.Distinct().Count().Should().Be(result.Count());
        }

        [Fact]
        public void GetSubscriberAddressesForMessage_NullMessages_Throws()
        {
            _persister.Invoking(p => p.GetSubscriberAddressesForMessage(null))
                .ShouldThrow<ArgumentNullException>();

            _mockFactory.Verify(m => m.CreateSubscriptionDbContext(), Times.Never());
        }

        [Fact]
        public void GetSubscriberAddressesForMessage_EmptyMessages_ReturnsEmpty()
        {
            var result = _persister.GetSubscriberAddressesForMessage(new List<MessageType>());

            _mockFactory.Verify(m => m.CreateSubscriptionDbContext(), Times.Never());
            result.Should().NotBeNull();
            result.Count().Should().Be(0);
        }

        private void AddSubscriptions()
        {
            _dbContext.Subscriptions.AddRange(
                new List<SubscriptionEntity>
                {
                    new SubscriptionEntity
                    {
                        MessageType = new MessageType(typeof(TestMessage)).ToString(),
                        SubscriberEndpoint = "queue@machine"
                    },
                    new SubscriptionEntity
                    {
                        MessageType = new MessageType(typeof(TestMessage2)).ToString(),
                        SubscriberEndpoint = "queue@machine"
                    },
                    new SubscriptionEntity
                    {
                        MessageType = new MessageType(typeof(TestMessage2)).ToString(),
                        SubscriberEndpoint = "queue@machine2"
                    },
                    new SubscriptionEntity
                    {
                        MessageType = new MessageType(typeof(TestMessage2)).ToString(),
                        SubscriberEndpoint = "queue@machine3"
                    },
                    new SubscriptionEntity
                    {
                        MessageType = new MessageType(typeof(TestMessage2)).ToString(),
                        SubscriberEndpoint = "queue@machine4"
                    },
                });

            _dbContext.SaveChanges();
        }
    }
}