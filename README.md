# NServiceBus EntityFramework Persistence
This is a plugin for NServiceBus to use EntityFramework for your persistence layer.

## Features Supported
 - Saga Storage
 - Subscription Storage
 - Timeout Storage

## Installation
    PM> Install-Package GoodlyFere.NServiceBus.EntityFramework -Pre

## Usage
The first step in using EntityFramework persistence is to configure your bus endpoint to use
this persistence library.

### Bus Configuration
When manually creating a bus configuration:

    BusConfiguration configuration = new BusConfiguration();
    configuration.UsePersistence<EntityFrameworkPersistence>();

Or, within an EndpointConfig:

    public class EndpointConfig
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UsePersistence<EntityFrameworkPersistence>();
        }
    }

### EntityFramework DbContext
The internal persisters in this library depend on an interface of type `INServiceBusDbContextFactory` that 
_you_ must implement in your client code.

The definition of this interface is very simple.

	
    public interface INServiceBusDbContextFactory
    {
        ISagaDbContext CreateSagaDbContext();

        ISubscriptionDbContext CreateSubscriptionDbContext();

        ITimeoutDbContext CreateTimeoutDbContext();
    }

Each method is intended to return an instance of an EntityFramework `DbContext` that defines access to the
database in which you want to store Saga data, subscriptions and timeouts.

Each interface defines the methods or properties that your `DbContext` must implement.  You can have three
different `DbContext`s or have one `DbContext` implement all three interfaces.  Here is an example of a `DbContext` that implements all three interfaces.

	public class ExampleDbContext : DbContext,
        ITimeoutDbContext,
        ISubscriptionDbContext,
        ISagaDbContext
    {
        public ExampleDbContext()
            : base("ExampleDbContext")
        {
        }

        public override DbSet SagaSet(Type sagaDataType)
        {
            if (sagaDataType == typeof(Example1SagaData))
            {
                return Example1Sagas;
            }

            if (sagaDataType == typeof(Example2SagaData))
            {
                return Example2Sagas;
            }

            return base.SagaSet(sagaDataType);
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
        public DbSet<TimeoutDataEntity> Timeouts { get; set; }

        public DbSet<Example1SagaData> Example1Sagas { get; set; }
        public DbSet<Example2SagaData> Example2Sagas { get; set; }
    }

In this example, I have two Saga data classes defined: `Example1Sagas` and `Example2Sagas`.  The SagaSet method 
determines which DbSet to return when asked.

Notice that the `SubscriptionEntity` and `TimeoutDataEntity` classes are defined in this library.

### Registration
You must then register your implementation of `INServiceBusDbContextFactory` with the dependency injection
framework that you have configured for NServiceBus.

## Version History
 - 1.0: Initial release to NuGet.  Stable use in production for timeout, subscription and saga data storage.