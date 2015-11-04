namespace GoodlyFere.NServiceBus.EntityFramework
{
    public static class ContextKeys
    {
        public const string SagaDbContextKey = "EntityFramework-SagaDbContext";
        public const string SagaTransactionKey = "EntityFramework-SagaDbContextTransaction";
    }
}