#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISubscriptionDbContext.cs">
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
// <created>03/26/2015 9:36 AM</created>
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Data.Entity;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Interfaces
{
    public interface ISubscriptionDbContext : INServiceBusDbContext
    {
        DbSet<SubscriptionEntity> Subscriptions { get; set; }
    }
}