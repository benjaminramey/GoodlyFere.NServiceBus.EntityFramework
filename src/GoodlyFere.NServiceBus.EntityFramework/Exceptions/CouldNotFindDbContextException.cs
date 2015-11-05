using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodlyFere.NServiceBus.EntityFramework.Exceptions
{
    public class CouldNotFindDbContextException : Exception
    {
        public CouldNotFindDbContextException(string dbContextName)
            : base(string.Format("Could not find {0} DbContext in session.", dbContextName))
        {
        }
    }
}