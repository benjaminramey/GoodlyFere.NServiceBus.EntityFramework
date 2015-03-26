#region Usings

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage
{
    public class SubscriptionEntity
    {
        [Key]
        public string MessageType { get; set; }

        [Key]
        public string SubscriberEndpoint { get; set; }
    }
}