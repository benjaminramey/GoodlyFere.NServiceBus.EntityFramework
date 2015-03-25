#region Usings

using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    /// <summary>
    ///     EntityFramework timeout storage NServiceBus feature.
    /// </summary>
    public class EntityFrameworkTimeoutStorage : Feature
    {
        public EntityFrameworkTimeoutStorage()
        {
            DependsOn<TimeoutManager>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<TimeoutPersister>(DependencyLifecycle.InstancePerCall);
        }
    }
}