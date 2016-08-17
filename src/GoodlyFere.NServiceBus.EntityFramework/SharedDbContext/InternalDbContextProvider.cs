using System;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Exceptions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    internal class InternalDbContextProvider : IDbContextProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger<InternalDbContextProvider>();

        public PipelineExecutor PipelineExecutor { get; set; }

        public ISagaDbContext GetSagaDbContext()
        {
            Logger.Debug("Getting DbContext");

            Lazy<ISagaDbContext> context;

            bool foundContext = PipelineExecutor.CurrentContext.TryGet(ContextKeys.SagaDbContextKey, out context);
            if (!foundContext)
            {
                Logger.Warn("Couldn't find DbContext in current context! Throwing an exception.");
                throw new CouldNotFindDbContextException(typeof(ISagaDbContext).Name);
            }

            Logger.Debug("Found DbContext in current context, returning it.");
            return context.Value;
        }
    }
}