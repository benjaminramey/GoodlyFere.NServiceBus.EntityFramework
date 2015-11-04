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
            Lazy<ISagaDbContext> context;

            bool foundContext = PipelineExecutor.CurrentContext.TryGet(ContextKeys.SagaDbContextKey, out context);
            if (!foundContext)
            {
                throw new CouldNotFindDbContextException(typeof(ISagaDbContext).Name);
            }

            return context.Value;
        }
    }
}