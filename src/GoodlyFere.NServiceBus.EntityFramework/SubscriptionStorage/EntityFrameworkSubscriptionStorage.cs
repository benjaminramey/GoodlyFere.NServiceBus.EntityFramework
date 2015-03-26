#region Usings

using System;
using System.Linq;
using NServiceBus.Features;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage
{
    public class EntityFrameworkSubscriptionStorage : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
        }
    }
}