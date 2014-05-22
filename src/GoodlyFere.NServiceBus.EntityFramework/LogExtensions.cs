#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="LogExtensions.cs">
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
using System.Collections.Concurrent;
using System.Linq;
using LoggingExtensions.Logging;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    /// <summary>
    ///     Extensions to help make logging awesome - this should be installed into the root namespace of your application
    /// </summary>
    public static class LogExtensions
    {
        #region Constants and Fields

        /// <summary>
        ///     Concurrent dictionary that ensures only one instance of a logger for a type.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ILog> _dictionary =
            new ConcurrentDictionary<string, ILog>();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the logger for <see cref="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type to get the logger for.</param>
        /// <returns>Instance of a logger for the object.</returns>
        public static ILog Log<T>(this T type)
        {
            string objectName = typeof(T).FullName;
            return Log(objectName);
        }

        /// <summary>
        ///     Gets the logger for the specified object name.
        /// </summary>
        /// <param name="objectName">Either use the fully qualified object name or the short. If used with Log&lt;T&gt;() you must use the fully qualified object name"/></param>
        /// <returns>Instance of a logger for the object.</returns>
        public static ILog Log(this string objectName)
        {
            return _dictionary.GetOrAdd(objectName, LoggingExtensions.Logging.Log.GetLoggerFor);
        }

        #endregion
    }
}