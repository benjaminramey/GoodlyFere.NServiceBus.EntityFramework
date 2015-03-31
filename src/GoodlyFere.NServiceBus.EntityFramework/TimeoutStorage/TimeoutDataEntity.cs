#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeoutDataEntity.cs">
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
// <created>03/25/2015 10:24 AM</created>
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    public class TimeoutDataEntity
    {
        /// <summary>
        ///     Id of this timeout.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     The address of the client who requested the timeout.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        ///     The saga ID.
        /// </summary>
        public Guid SagaId { get; set; }

        /// <summary>
        ///     Additional state.
        /// </summary>
        public byte[] State { get; set; }

        /// <summary>
        ///     The time at which the saga ID expired.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        ///     Store the headers to preserve them across timeouts.
        /// </summary>
        public string Headers { get; set; }

        /// <summary>
        ///     Timeout endpoint name.
        /// </summary>
        public string Endpoint { get; set; }
    }
}