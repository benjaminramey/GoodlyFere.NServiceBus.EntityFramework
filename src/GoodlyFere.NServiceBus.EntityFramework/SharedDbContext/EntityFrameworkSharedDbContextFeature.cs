using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    public class EntityFrameworkSharedDbContextFeature : Feature
    {
        internal EntityFrameworkSharedDbContextFeature()
        {
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<CreateDbContextBehavior>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<InternalDbContextProvider>(DependencyLifecycle.SingleInstance);

            context.Pipeline.Register<CreateDbContextBehavior.Registration>();
        }
    }
}