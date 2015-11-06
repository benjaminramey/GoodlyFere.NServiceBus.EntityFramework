using System;
using System.Linq;

namespace GoodlyFere.NServiceBus.EntityFramework.Exceptions
{
    public class UpdatingDetachedEntityException : Exception
    {
        public UpdatingDetachedEntityException()
            : base("Cannot update a detached entity.")
        {
        }
    }
}