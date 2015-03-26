#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="TimeoutDataEntity.cs">
//  GoodlyFere.NServiceBus.EntityFramework
//  
//  Copyright (C) 2014 
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//  
//  http://www.gnu.org/licenses/lgpl-2.1-standalone.html
//  
//  You can contact me at ben.ramey@gmail.com.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NServiceBus;

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