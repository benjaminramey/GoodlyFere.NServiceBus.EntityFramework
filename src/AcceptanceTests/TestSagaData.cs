using System;
using System.Linq;
using NServiceBus.Saga;

namespace AcceptanceTests
{
    public class TestSagaData : ContainSagaData
    {
        public string Name { get; set; }
    }
}