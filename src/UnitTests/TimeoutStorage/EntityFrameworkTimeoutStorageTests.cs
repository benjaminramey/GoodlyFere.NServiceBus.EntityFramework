#region Usings

using System;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;
using Xunit;

#endregion

namespace UnitTests.TimeoutStorage
{
    public class EntityFrameworkTimeoutStorageTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action action = () => new EntityFrameworkTimeoutStorage();

            action.Invoking(a => a.Invoke()).ShouldNotThrow();
        }
    }
}