using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public static class ContextKeys
    {
        public const string TimeoutDbContextKey = "EntityFramework-TimeoutDbContext";
        public const string SagaDbContextKey = "EntityFramework-SagaDbContext";
        public const string SubscriptionDbContextKey = "EntityFramework-SubscriptionDbContext";
        public const string SharedDbContextSetupFlagKey = "EntityFramework-SharedDbContextSetupFlag";

        public const string SagaTransactionKey = "EntityFramework-SagaDbContextTransaction";
        public const string TimeoutTransactionKey = "EntityFramework-TimeoutDbContextTransaction";
        public const string SubscriptionTransactionKey = "EntityFramework-SubscriptionDbContextTransaction";
    }
}