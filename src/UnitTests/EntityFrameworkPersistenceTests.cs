#region Usings

using System;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework;
using NServiceBus.Persistence;
using Xunit;

#endregion

namespace UnitTests
{
    public class EntityFrameworkPersistenceTests
    {
        [Fact]
        public void SupportsTimeouts()
        {
            var obj = new EntityFrameworkPersistence();
            obj.HasSupportFor<StorageType.Timeouts>().Should().BeTrue();
        }

        [Fact]
        public void SupportsSubscriptions()
        {
            var obj = new EntityFrameworkPersistence();
            obj.HasSupportFor<StorageType.Subscriptions>().Should().BeTrue();
        }

        [Fact]
        public void SupportsSagas()
        {
            var obj = new EntityFrameworkPersistence();
            obj.HasSupportFor<StorageType.Sagas>().Should().BeTrue();
        }

        [Fact]
        public void DoesNotSupportOutbox()
        {
            var obj = new EntityFrameworkPersistence();
            obj.HasSupportFor<StorageType.Outbox>().Should().BeFalse();
        }
    }
}