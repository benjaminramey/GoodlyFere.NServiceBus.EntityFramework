#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GoodlyFere.NServiceBus.EntityFramework.Support;
using Newtonsoft.Json;
using Xunit;

#endregion

namespace UnitTests.Support
{
    public class SerializeHelperTests
    {
        [Fact]
        public void ToDictionary_Null_ReturnsNewDictionary()
        {
            var result = SerializeHelper.ToDictionary(null);

            result.Should().NotBeNull();
        }

        [Fact]
        public void ToDictionary_Empty_ReturnsNewDictionary()
        {
            var result = SerializeHelper.ToDictionary(string.Empty);

            result.Should().NotBeNull();
        }

        [Fact]
        public void ToDictionary_Valid_ReturnsMatchingDictionary()
        {
            Dictionary<string, string> expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };
            string expectedStr = JsonConvert.SerializeObject(expected);

            var result = SerializeHelper.ToDictionary(expectedStr);

            result.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void ToDictionaryString_Valid_ReturnsSerializedString()
        {
            Dictionary<string, string> expected = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };
            string expectedStr = JsonConvert.SerializeObject(expected);

            var result = SerializeHelper.ToDictionaryString(expected);

            result.ShouldBeEquivalentTo(expectedStr);
        }

        [Fact]
        public void ToDictionaryString_Null_ReturnsNull()
        {
            var result = SerializeHelper.ToDictionaryString(null);

            result.Should().BeNull();
        }

        [Fact]
        public void ToDictionaryString_Empty_ReturnsNull()
        {
            var result = SerializeHelper.ToDictionaryString(new Dictionary<string, string>());

            result.Should().BeNull();
        }
    }
}