#region Usings

using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using Moq;

#endregion

namespace UnitTests.SubscriptionStorage
{
    public class SubscriptionPersisterTests
    {
        private readonly Mock<INServiceBusDbContextFactory> _mockFactory;
        private readonly SubscriptionPersister _persister;

        public SubscriptionPersisterTests()
        {
            _mockFactory = new Mock<INServiceBusDbContextFactory>();
            _mockFactory.Setup(m => m.CreateSubscriptionDbContext()).Returns(new TestSubscriptionDbContext());

            _persister = new SubscriptionPersister(_mockFactory.Object);
        }
    }
}