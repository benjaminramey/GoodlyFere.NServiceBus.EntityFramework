using System;
using System.Data.Entity;
using System.Transactions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using IsolationLevel = System.Data.IsolationLevel;

namespace GoodlyFere.NServiceBus.EntityFramework.SharedDbContext
{
    internal class CreateDbContextBehavior : IBehavior<IncomingContext>
    {
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
            Lazy<ISagaDbContext> lazyDbContext;

            if (context.TryGet(ContextKeys.SagaDbContextKey, out lazyDbContext))
            {
                next();
                return;
            }

            lazyDbContext = CreateLazySagaDbContext(context);
            context.Set(ContextKeys.SagaDbContextKey, lazyDbContext);

            try
            {
                next();

                if (lazyDbContext.IsValueCreated)
                {
                    FinishTransaction(context);
                }
            }
            finally
            {
                if (lazyDbContext.IsValueCreated)
                {
                    DisposeTransaction(context);

                    lazyDbContext.Value.Dispose();
                }

                context.Remove(ContextKeys.SagaDbContextKey);
            }
        }

        private Lazy<ISagaDbContext> CreateLazySagaDbContext(IncomingContext context)
        {
            Func<ISagaDbContext> lazyFunc =
                () =>
                {
                    ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext();

                    if (Transaction.Current == null)
                    {
                        DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.Serializable);
                        context.Set(ContextKeys.SagaTransactionKey, transaction);
                    }

                    return dbc;
                };

            return new Lazy<ISagaDbContext>(lazyFunc);
        }

        private void DisposeTransaction(IncomingContext context)
        {
            DbContextTransaction transaction;
            if (context.TryGet(ContextKeys.SagaTransactionKey, out transaction))
            {
                transaction.Dispose();
            }
        }

        private static void FinishTransaction(IncomingContext context)
        {
            DbContextTransaction transaction;
            if (context.TryGet(ContextKeys.SagaTransactionKey, out transaction))
            {
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
                InsertAfter(WellKnownStep.ExecuteUnitOfWork);
                InsertAfterIfExists("OutboxDeduplication");
                InsertBefore(WellKnownStep.MutateIncomingTransportMessage);
                InsertBeforeIfExists("OutboxRecorder");
                InsertBeforeIfExists(WellKnownStep.InvokeSaga);
            }
        }
    }
}