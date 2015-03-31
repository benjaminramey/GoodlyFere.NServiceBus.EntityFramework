#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializeHelperTests.cs">
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
// <created>03/26/2015 1:35 PM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
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