#region Usings

using System;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using Moq;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;
using NSubstitute;
using Xunit;

#endregion

namespace UnitTests.SagaStorage
{
    public class EntityFrameworkSagaStorageTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action action = () => new EntityFrameworkSagaStorage();

            action.Invoking(a => a.Invoke()).ShouldNotThrow();
        }
    }
}