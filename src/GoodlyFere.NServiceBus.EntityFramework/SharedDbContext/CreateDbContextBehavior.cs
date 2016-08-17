using System;
using System.Data.Entity;
using System.Transactions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using IsolationLevel = System.Data.IsolationLevel;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    internal class CreateDbContextBehavior : IBehavior<IncomingContext>
    {
        private static readonly ILog Logger = LogManager.GetLogger<CreateDbContextBehavior>();

        private readonly INServiceBusDbContextFactory _dbContextFactory;

        public CreateDbContextBehavior(INServiceBusDbContextFactory dbContextFactory)
        {
            if (dbContextFactory == null)
            {
                throw new ArgumentNullException("dbContextFactory");
            }
            _dbContextFactory = dbContextFactory;
        }

        public void Invoke(IncomingContext context, Action next)
        {
            Logger.Debug("Invoking CreateDbContextBehavior");
            Lazy<ISagaDbContext> lazyDbContext;

            if (context.TryGet(ContextKeys.SagaDbContextKey, out lazyDbContext))
            {
                Logger.Debug("Lazy DbContext already exists in context, calling next().");
                next();
                return;
            }

            Logger.Debug("Lazy DbContext does not exist in context.");
            lazyDbContext = CreateLazySagaDbContext(context);
            context.Set(ContextKeys.SagaDbContextKey, lazyDbContext);

            try
            {
                Logger.Debug("Calling next");
                next();

                Logger.Debug("Checking if lazyDbContext has a value");
                if (lazyDbContext.IsValueCreated)
                {
                    Logger.Debug("lazyDbContext does have a value");
                    FinishTransaction(context);
                }
            }
            finally
            {
                Logger.Debug("Reached finally block after caling next()");

                Logger.Debug("Checking if lazyDbContext has a value");
                if (lazyDbContext.IsValueCreated)
                {
                    Logger.Debug("lazyDbContext does have a value");
                    DisposeTransaction(context);
                    lazyDbContext.Value.Dispose();
                }

                Logger.Debug("Removing SagaDbContextKey from context.");
                context.Remove(ContextKeys.SagaDbContextKey);
            }
        }

        private Lazy<ISagaDbContext> CreateLazySagaDbContext(IncomingContext context)
        {
            Func<ISagaDbContext> lazyFunc =
                () =>
                {
                    Logger.Debug("Lazy ISagaDbContext func called");
                    ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext();

                    Logger.Debug("Checking if Transaction.Current has a value");
                    if (Transaction.Current == null)
                    {
                        Logger.Debug("Current transaction does NOT exist, creating new transaction with Serializable isolation level");
                        DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.Serializable);
                        context.Set(ContextKeys.SagaTransactionKey, transaction);
                    }

                    return dbc;
                };

            return new Lazy<ISagaDbContext>(lazyFunc);
        }

        private void DisposeTransaction(IncomingContext context)
        {
            Logger.Debug("Disposing transaction");
            DbContextTransaction transaction;
            if (context.TryGet(ContextKeys.SagaTransactionKey, out transaction))
            {
                Logger.Debug("Found transaction in context, disposing");
                transaction.Dispose();
            }
        }

        private static void FinishTransaction(IncomingContext context)
        {
            Logger.Debug("Finishing transaction");
            DbContextTransaction transaction;
            if (context.TryGet(ContextKeys.SagaTransactionKey, out transaction))
            {
                Logger.Debug("Found transaction in context, committing and disposing");
                transaction.Commit();
                transaction.Dispose();
            }
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base(
                    "CreateDbContext",
                    typeof(CreateDbContextBehavior),
                    "Makes sure that there is a DbContext available on the pipeline")
            {
                Logger.Debug("Registering CreateDbContext behavior");

                InsertAfter(WellKnownStep.ExecuteUnitOfWork);
                InsertAfterIfExists("OutboxDeduplication");
                InsertBefore(WellKnownStep.MutateIncomingTransportMessage);
                InsertBeforeIfExists("OutboxRecorder");
                InsertBeforeIfExists(WellKnownStep.InvokeSaga);
            }
        }
    }
}