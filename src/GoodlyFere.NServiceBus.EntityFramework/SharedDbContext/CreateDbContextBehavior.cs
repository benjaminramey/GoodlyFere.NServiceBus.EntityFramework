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
            bool hasAlreadySetupFlag = context.TryGet(ContextKeys.SharedDbContextSetupFlagKey, out alreadySetup);

            if (hasAlreadySetupFlag && alreadySetup)
            {
                next();
                return;
            }

            Lazy<ISagaDbContext> lazySagaCreation = CreateLazySagaDbContext(context);
            context.Set(ContextKeys.SagaDbContextKey, lazySagaCreation);
            context.Set(ContextKeys.SharedDbContextSetupFlagKey, true);

            try
            {
                next();
                FinishTransaction(context);
            }
            catch (Exception)
            {
                RollbackTransaction(context);
                context.Remove(ContextKeys.SagaTransactionKey);
                throw;
            }
            finally
            {
                DisposeDbContext(lazySagaCreation);

                context.Remove(ContextKeys.SagaDbContextKey);
                context.Remove(ContextKeys.SharedDbContextSetupFlagKey);
            }
        }

        private Lazy<ISagaDbContext> CreateLazySagaDbContext(IncomingContext context)
        {
            Func<ISagaDbContext> lazyFunc =
                () =>
                {
                    ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext();
                    DbContextTransaction transaction = dbc.Database.BeginTransaction(IsolationLevel.Serializable);

                    context.Set(ContextKeys.SagaTransactionKey, transaction);

                    return dbc;
                };

            return new Lazy<ISagaDbContext>(lazyFunc);
        }

        private static void DisposeDbContext<TContext>(Lazy<TContext> lazyDbContextCreation)
            where TContext : IDisposable
        {
            if (lazyDbContextCreation.IsValueCreated)
            {
                lazyDbContextCreation.Value.Dispose();
            }
        }

        private static void FinishTransaction(IncomingContext context)
        {
            DbContextTransaction transaction;
            if (context.TryGet(ContextKeys.SagaTransactionKey, out transaction))
            {
                transaction.Commit();
                transaction.Dispose();

                context.Remove(ContextKeys.SagaTransactionKey);
            }
        }

        private void RollbackTransaction(IncomingContext context)
        {
            DbContextTransaction transaction;
            if (context.TryGet(ContextKeys.SagaTransactionKey, out transaction))
            {
                transaction.Rollback();
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