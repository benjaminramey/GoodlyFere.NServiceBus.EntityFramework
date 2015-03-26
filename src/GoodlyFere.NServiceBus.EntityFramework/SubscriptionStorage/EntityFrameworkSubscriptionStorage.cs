#region Usings

using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SubscriptionStorage
{
    public class EntityFrameworkSubscriptionStorage : Feature
    {
        public EntityFrameworkSubscriptionStorage()
        {
            DependsOn<StorageDrivenPublishing>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<SubscriptionPersister>(DependencyLifecycle.InstancePerCall);
        }
    }
}