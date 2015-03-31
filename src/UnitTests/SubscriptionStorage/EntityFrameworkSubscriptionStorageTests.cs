#region Usings

using System;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using Xunit;

#endregion

namespace UnitTests.SubscriptionStorage
{
    public class EntityFrameworkSubscriptionStorageTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action action = () => new EntityFrameworkSubscriptionStorage();

            action.Invoking(a => a.Invoke()).ShouldNotThrow();
        }
    }
}