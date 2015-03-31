#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializeHelper.cs">
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
// <created>03/25/2015 4:41 PM</created>
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus.Serializers.Json;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Support
{
    internal static class SerializeHelper
    {
        private static readonly JsonMessageSerializer Serializer = new JsonMessageSerializer(null);

        internal static Dictionary<string, string> ToDictionary(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return new Dictionary<string, string>();
            }

            return (Dictionary<string, string>)Serializer.DeserializeObject(str, typeof(Dictionary<string, string>));
        }

        internal static string ToDictionaryString(this Dictionary<string, string> dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                return Serializer.SerializeObject(dictionary);
            }

            return null;
        }
    }
}