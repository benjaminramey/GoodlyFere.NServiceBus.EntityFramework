#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkTimeoutStorage.cs">
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
// <created>03/25/2015 9:59 AM</created>
// <updated>03/31/2015 12:50 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    /// <summary>
    ///     EntityFramework timeout storage NServiceBus feature.
    /// </summary>
    public class EntityFrameworkTimeoutStorage : Feature
    {
        public EntityFrameworkTimeoutStorage()
        {
            DependsOn<TimeoutManager>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container
                .ConfigureComponent<TimeoutPersister>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(p => p.EndpointName, context.Settings.EndpointName());
        }
    }
}