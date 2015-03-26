#region Usings

using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public static class SharedConfig
    {
        /// <summary>
        ///     Sets the connection string to use for all storages
        /// </summary>
        /// <param name="persistenceConfiguration"></param>
        /// <param name="connectionString">The connection string to use.</param>
        public static PersistenceExtentions<EntityFrameworkPersistence> ConnectionString(
            this PersistenceExtentions<EntityFrameworkPersistence> persistenceConfiguration,
            string connectionString)
        {
            persistenceConfiguration.GetSettings().Set("EntityFramework.Common.ConnectionString", connectionString);
            return persistenceConfiguration;
        }
    }
}