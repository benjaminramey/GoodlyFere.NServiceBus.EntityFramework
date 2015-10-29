# NServiceBus EntityFramework Persistence
This is a plugin for NServiceBus to use EntityFramework for your persistence layer.

## Features Supported
 - Saga Storage
 - Subscription Storage
 - Timeout Storage

## Installation
    PM> Install-Package GoodlyFere.NServiceBus.EntityFramework -Pre

## Usage
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

## Version History
 - 1.0: Initial release to NuGet.  Stable use in production for timeout, subscription and saga data storage.