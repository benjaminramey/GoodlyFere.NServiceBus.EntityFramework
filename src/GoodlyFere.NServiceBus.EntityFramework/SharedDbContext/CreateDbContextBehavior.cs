using System;
using System.Data;
using System.Data.Entity;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;

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
            bool alreadySetup;
            if (context.TryGet(ContextKeys.SharedDbContextSetupFlagKey, out alreadySetup)
                && alreadySetup)
            {
                next();
                return;
            }

            Lazy<ITimeoutDbContext> lazyTimeoutCreation = CreateLazyTimeoutDbContext(context);
            Lazy<ISagaDbContext> lazySagaCreation = CreateLazySagaDbContext(context);
            Lazy<ISubscriptionDbContext> lazySubscriptionCreation = CreateLazySubscriptionDbContext(context);

            context.Set(ContextKeys.TimeoutDbContextKey, lazyTimeoutCreation);
            context.Set(ContextKeys.SagaDbContextKey, lazySagaCreation);
            context.Set(ContextKeys.SubscriptionDbContextKey, lazySubscriptionCreation);

            try
            {
                next();

                FinishTransaction(context, ContextKeys.TimeoutTransactionKey);
                FinishTransaction(context, ContextKeys.SagaTransactionKey);
                FinishTransaction(context, ContextKeys.SubscriptionTransactionKey);
            }
            catch (Exception)
            {
                RollbackTransaction(context, ContextKeys.TimeoutTransactionKey);
                RollbackTransaction(context, ContextKeys.SagaTransactionKey);
                RollbackTransaction(context, ContextKeys.SubscriptionTransactionKey);

                throw;
            }
            finally
            {
                DisposeDbContext(lazyTimeoutCreation);
                DisposeDbContext(lazySagaCreation);
                DisposeDbContext(lazySubscriptionCreation);

                context.Remove(ContextKeys.TimeoutDbContextKey);
                context.Remove(ContextKeys.SagaDbContextKey);
                context.Remove(ContextKeys.SubscriptionDbContextKey);
            }
        }

        private Lazy<ISagaDbContext> CreateLazySagaDbContext(IncomingContext context)
        {
            Func<ISagaDbContext> lazyFunc =
                () =>
                {
                    ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext();
                    DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.RepeatableRead);

                    context.Set(ContextKeys.SagaTransactionKey, transaction);

                    return dbc;
                };

            return new Lazy<ISagaDbContext>(lazyFunc);
        }

        private Lazy<ISubscriptionDbContext> CreateLazySubscriptionDbContext(IncomingContext context)
        {
            Func<ISubscriptionDbContext> lazyFunc =
                () =>
                {
                    ISubscriptionDbContext dbc = _dbContextFactory.CreateSubscriptionDbContext();
                    DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted);

                    context.Set(ContextKeys.SubscriptionTransactionKey, transaction);

                    return dbc;
                };

            return new Lazy<ISubscriptionDbContext>(lazyFunc);
        }

        private Lazy<ITimeoutDbContext> CreateLazyTimeoutDbContext(IncomingContext context)
        {
            Func<ITimeoutDbContext> lazyFunc =
                () =>
                {
                    ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext();
                    DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted);

                    context.Set(ContextKeys.TimeoutTransactionKey, transaction);

                    return dbc;
                };

            return new Lazy<ITimeoutDbContext>(lazyFunc);
        }

        private static void DisposeDbContext<TContext>(Lazy<TContext> lazyDbContextCreation)
            where TContext : IDisposable
        {
            if (lazyDbContextCreation.IsValueCreated)
            {
                lazyDbContextCreation.Value.Dispose();
            }
        }

        private static void FinishTransaction(
            IncomingContext context,
            string transactionKey)
        {
            DbContextTransaction transaction;
            if (context.TryGet(transactionKey, out transaction))
            {
                transaction.Commit();
                transaction.Dispose();

                context.Remove(transactionKey);
            }
        }

        private void RollbackTransaction(IncomingContext context, string transactionKey)
        {
            DbContextTransaction transaction;
            if (context.TryGet(transactionKey, out transaction))
            {
                transaction.Rollback();
                transaction.Dispose();

                context.Remove(transactionKey);
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