#region Usings

using System;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.Support;
using Xunit;

#endregion

namespace UnitTests.Support
{
    public class CombGuidTests
    {
        [Fact]
        public void NewGuid_ProducesSequentialGuids()
        {
            var guid1 = CombGuid.NewGuid().ToByteArray();
            var guid2 = CombGuid.NewGuid().ToByteArray();

            var guid1Int = BitConverter.ToInt32(guid1, guid1.Length - 4);
            var guid2Int = BitConverter.ToInt32(guid2, guid2.Length - 4);

            guid1Int.Should().BeLessOrEqualTo(guid2Int);
        }
    }
}