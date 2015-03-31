#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkPersistence.cs">
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
// <created>03/25/2015 9:41 AM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.SagaStorage;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;
using GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage;
using NServiceBus.Features;
using NServiceBus.Persistence;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    /// <summary>
    ///     EntityFramework persistence for NServiceBus.
    /// </summary>
    public class EntityFrameworkPersistence : PersistenceDefinition
    {
        /// <summary>
        ///     Defines the capabilities of the storage
        /// </summary>
        public EntityFrameworkPersistence()
        {
            Supports<StorageType.Timeouts>(s => s.EnableFeatureByDefault<EntityFrameworkTimeoutStorage>());
            Supports<StorageType.Subscriptions>(s => s.EnableFeatureByDefault<EntityFrameworkSubscriptionStorage>());
            Supports<StorageType.Sagas>(s => s.EnableFeatureByDefault<EntityFrameworkSagaStorage>());
        }
    }
}