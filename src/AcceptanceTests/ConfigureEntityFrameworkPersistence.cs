using AcceptanceTests;
using GoodlyFere.NServiceBus.EntityFramework;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus;

public class ConfigureEntityFrameworkPersistence
{
    public void Configure(BusConfiguration config)
    {
        config.RegisterComponents(
            c => c.ConfigureComponent<TestDbContextFactory>(DependencyLifecycle.SingleInstance));
        config.UsePersistence<EntityFrameworkPersistence>();
    }
}