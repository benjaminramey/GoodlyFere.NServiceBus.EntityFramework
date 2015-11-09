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
There are a couple of integration points that you must address in your
client code.  You must implement your EntityFramework `DbContext` and
you must implement the `INServiceBusDbContextFactory` interface that
this library provides you.

#### INServiceBusDbContextFactory
The internal persisters in this library depend on an interface of type `INServiceBusDbContextFactory` that 
_you_ must implement in your client code.

##### Implementation
The definition of this interface is very simple.

	
    public interface INServiceBusDbContextFactory
    {
        ISagaDbContext CreateSagaDbContext();

        ISubscriptionDbContext CreateSubscriptionDbContext();

        ITimeoutDbContext CreateTimeoutDbContext();
    }

Each method is intended to return an instance of an EntityFramework `DbContext` that defines access to the
database in which you want to store Saga data, subscriptions and timeouts.

Each interface defines the methods or properties that your `DbContext` must implement.

##### Container Registration
You must also register your implementation of `INServiceBusDbContextFactory`
with the dependency injection framework that you have configured for NServiceBus.

If you are using the default container with NServiceBus, your registration
may look something like this.

	public class EndpointConfig
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.RegisterComponents(c => c.ConfigureComponent<MyNServiceBusDbContextFactory>(DependencyLifecycle.SingleInstance));
			
            configuration.UsePersistence<EntityFrameworkPersistence>();
        }
    }

#### DbContext
You can have three different `DbContext`s (one for each interface that the
`INServiceBusDbContextFactory` requires) or have one `DbContext` implement all three interfaces.  Here is an example of a `DbContext` that implements all three interfaces.

	public class ExampleDbContext : DbContext,
        ITimeoutDbContext,
        ISubscriptionDbContext,
        ISagaDbContext
    {
        public ExampleDbContext()
            : base("ExampleDbContext")
        {
        }

        public bool HasSet(Type entityType)
        {
            return Set(entityType) != null;
        }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }
        public DbSet<TimeoutDataEntity> Timeouts { get; set; }

        public DbSet<Example1SagaData> Example1Sagas { get; set; }
        public DbSet<Example2SagaData> Example2Sagas { get; set; }
    }

In this example, I have two Saga data classes defined: `Example1Sagas` and `Example2Sagas`.  The `HasSet` method is defined in `ISagaDbContext` and 
should return true if you have a `DbSet<>` property defined on your `DbContext`
for the type provided.

Notice that the `SubscriptionEntity` and `TimeoutDataEntity` classes are defined in this library.

##### An Easier Way to Define Your DbContext
A good portion of the three interfaces and what they make you implement
is boilerplate, unless you have very specific use-cases.  They're also useful
to mock in testing, of course.

If you don't have some abnormal EntityFramework setup, you can have your custom
`DbContext` inherit from the abstract `NServiceBusDbContext` class in the
`GoodlyFere.NServiceBus.EntityFramework.SharedDbContext` namespace.  This
abstract class is simple in definition and takes care of everything except
defining your custom saga data `DbSet<>` properties.

	public abstract class NServiceBusDbContext : DbContext, ISubscriptionDbContext, ITimeoutDbContext, ISagaDbContext
    {
        protected NServiceBusDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public virtual bool HasSet(Type entityType)
        {
            return Set(entityType) != null;
        }

        public virtual DbSet<SubscriptionEntity> Subscriptions { get; set; }
        public virtual DbSet<TimeoutDataEntity> Timeouts { get; set; }
    }

## Version History
 - 1.0: Initial release to NuGet.  Stable use in production for timeout, subscription and saga data storage.