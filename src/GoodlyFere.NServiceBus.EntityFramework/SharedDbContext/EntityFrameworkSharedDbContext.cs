using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    public class EntityFrameworkSharedDbContext : Feature
    {
        internal EntityFrameworkSharedDbContext()
        {
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register<CreateDbContextBehavior.Registration>();

            context.Container.ConfigureComponent<InternalDbContextProvider>(DependencyLifecycle.SingleInstance);
        }
    }
}