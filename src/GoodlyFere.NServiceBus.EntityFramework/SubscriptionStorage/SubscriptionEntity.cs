#region Usings

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage
{
    public class SubscriptionEntity
    {
        [Column(Order = 1)]
        [Key]
        public string MessageType { get; set; }

        [Column(Order = 0)]
        [Key]
        public string SubscriberEndpoint { get; set; }
    }
}