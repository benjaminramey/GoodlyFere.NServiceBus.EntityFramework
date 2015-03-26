#region Usings

using System;
using System.Linq;
using NServiceBus.Features;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.OutboxStorage
{
    public class EntityFrameworkOutboxStorage : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
        }
    }
}