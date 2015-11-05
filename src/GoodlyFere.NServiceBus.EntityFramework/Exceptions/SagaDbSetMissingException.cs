using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodlyFere.NServiceBus.EntityFramework.Exceptions
{
    public class SagaDbSetMissingException : Exception
    {
        public SagaDbSetMissingException(Type dbContextType, Type sagaType)
            : base(
                string.Format(
                    "{0} is missing a DbSet property for saga of type {1}.",
                    dbContextType.FullName,
                    sagaType.FullName))
        {
        }
    }
}