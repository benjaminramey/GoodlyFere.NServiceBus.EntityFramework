using System;
using System.Linq;

namespace GoodlyFere.NServiceBus.EntityFramework.Exceptions
{
    public class DeletingDetachedEntityException : Exception
    {
        public DeletingDetachedEntityException()
            : base("Cannot delete a detached entity.")
        {
        }
    }
}