using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Exceptions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus.Pipeline;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    internal class InternalDbContextProvider : IDbContextProvider
    {
        public PipelineExecutor PipelineExecutor { get; set; }

        public ISagaDbContext GetSagaDbContext()
        {
            return GetContextFromPipelineContext<ISagaDbContext>(ContextKeys.SagaDbContextKey);
        }

        public ISubscriptionDbContext GetSubscriptionDbContext()
        {
            return GetContextFromPipelineContext<ISubscriptionDbContext>(ContextKeys.SubscriptionDbContextKey);
        }

        public ITimeoutDbContext GetTimeoutDbContext()
        {
            return GetContextFromPipelineContext<ITimeoutDbContext>(ContextKeys.TimeoutDbContextKey);
        }

        private TContext GetContextFromPipelineContext<TContext>(string contextKey)
        {
            Lazy<TContext> context;

            bool foundContext = PipelineExecutor.CurrentContext.TryGet(contextKey, out context);
            if (!foundContext)
            {
                throw new CouldNotFindDbContextException(typeof(TContext).Name);
            }

            return context.Value;
        }
    }
}