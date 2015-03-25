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

        internal static string ToAddressString(this Address address)
        {
            if (address == null)
            {
                return null;
            }

            return Serializer.SerializeObject(address);
        }

        internal static Address ToAddress(this string addressString)
        {
            if (string.IsNullOrEmpty(addressString))
            {
                return null;
            }

            return (Address)Serializer.DeserializeObject(addressString, typeof(Address));
        }
    }
}