#region Usings

using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Features;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SagaStorage
{
    public class EntityFrameworkSagaStorage : Feature
    {
        public EntityFrameworkSagaStorage()
        {
            DependsOn<Sagas>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<SagaPersister>(DependencyLifecycle.InstancePerCall);
        }
    }
}