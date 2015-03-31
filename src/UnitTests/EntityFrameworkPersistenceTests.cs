#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkPersistenceTests.cs">
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
// <created>03/26/2015 12:17 PM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using FluentAssertions;
using System.Linq;
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