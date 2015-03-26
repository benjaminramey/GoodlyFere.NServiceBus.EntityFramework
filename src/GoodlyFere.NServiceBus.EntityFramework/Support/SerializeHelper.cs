#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;
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