#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SagaPersister.cs">
//  Copyright 2015 Benjamin S. Ramey
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </copyright>
// <created>03/25/2015 9:51 AM</created>
// <updated>03/31/2015 12:55 PM by Ben Ramey</updated>
// --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using GoodlyFere.NServiceBus.EntityFramework.Exceptions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using NServiceBus.Logging;
using NServiceBus.Saga;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SagaStorage
{
    public class SagaPersister : ISagaPersister
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SagaPersister));
        private readonly IDbContextProvider _dbContextProvider;
        private ISagaDbContext _dbContext;

        public SagaPersister(IDbContextProvider dbContextProvider)
        {
            if (dbContextProvider == null)
            {
                throw new ArgumentNullException("dbContextProvider");
            }

            _dbContextProvider = dbContextProvider;
        }

        private ISagaDbContext DbContext
        {
            get
            {
                return _dbContext ?? (_dbContext = _dbContextProvider.GetSagaDbContext());
            }
        }

        public void Complete(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            Type sagaType = GetSagaType(saga);
            if (!DbContext.HasSet(sagaType))
            {
                throw new SagaDbSetMissingException(DbContext.GetType(), sagaType);
            }

            DbEntityEntry entry = DbContext.Entry(saga);
            if (entry.State == EntityState.Detached)
            {
                throw new DeletingDetachedEntityException();
            }

            entry.Reload(); // avoid concurrency issues since we're deleting
            entry.State = EntityState.Deleted;
            DbContext.SaveChanges();
        }

        public TSagaData Get<TSagaData>(Guid sagaId) where TSagaData : IContainSagaData
        {
            Type sagaType = typeof(TSagaData);
            if (!DbContext.HasSet(sagaType))
            {
                throw new SagaDbSetMissingException(DbContext.GetType(), sagaType);
            }

            if (sagaId == Guid.Empty)
            {
                throw new ArgumentException("sagaId cannot be empty.", "sagaId");
            }

            DbSet set = DbContext.Set(sagaType);
            object result = set.Find(sagaId);
            return (TSagaData)(result ?? default(TSagaData));
        }

        public TSagaData Get<TSagaData>(string propertyName, object propertyValue) where TSagaData : IContainSagaData
        {
            Type sagaType = typeof(TSagaData);
            if (!DbContext.HasSet(sagaType))
            {
                throw new SagaDbSetMissingException(DbContext.GetType(), sagaType);
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            ParameterExpression param = Expression.Parameter(sagaType, "sagaData");
            Expression<Func<TSagaData, bool>> filter = Expression.Lambda<Func<TSagaData, bool>>(
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.Property(param, propertyName),
                    Expression.Constant(propertyValue)),
                param);

            IQueryable setQueryable = DbContext.Set(sagaType).AsQueryable();
            IQueryable result = setQueryable
                .Provider
                .CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        "Where",
                        new[] { sagaType },
                        setQueryable.Expression,
                        Expression.Quote(filter)));

            List<object> results = result.ToListAsync().Result;
            if (results.Any())
            {
                return (TSagaData)results.First();
            }

            return default(TSagaData);
        }

        public void Save(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            Type sagaType = GetSagaType(saga);
            if (!DbContext.HasSet(sagaType))
            {
                throw new SagaDbSetMissingException(DbContext.GetType(), sagaType);
            }

            DbSet sagaSet = DbContext.Set(sagaType);
            sagaSet.Add(saga);
            DbContext.SaveChanges();
        }

        public void Update(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            Type sagaType = GetSagaType(saga);
            if (!DbContext.HasSet(sagaType))
            {
                throw new SagaDbSetMissingException(DbContext.GetType(), sagaType);
            }

            try
            {
                DbEntityEntry entry = DbContext.Entry((object)saga);
                if (entry.State == EntityState.Detached)
                {
                    throw new UpdatingDetachedEntityException();
                }

                if (entry.State == EntityState.Modified)
                {
                    DbContext.SaveChanges();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (IsOptimisticConcurrencyException(ex))
                {
                    ReconcileConcurrencyIssues(saga);
                    DbContext.SaveChanges();
                }
                else
                {
                    throw;
                }
            }
        }

        private static Type GetSagaType(IContainSagaData saga)
        {
            Type sagaType = saga.GetType();

            // if this class is the dynamic proxy type inserted by EF
            // then we want the base type (actual saga data type) not the dynamic proxy
            if (typeof(IEntityWithChangeTracker).IsAssignableFrom(sagaType))
            {
                sagaType = sagaType.BaseType;
            }

            return sagaType;
        }

        private static bool IsOptimisticConcurrencyException(DbUpdateConcurrencyException ex)
        {
            return ex.InnerException != null
                   && ex.InnerException is OptimisticConcurrencyException;
        }

        private void ReconcileConcurrencyIssues(IContainSagaData saga)
        {
            DbEntityEntry entry = DbContext.Entry((object)saga);

            // 1. get the names of properties that have changes.
            List<string> changedPropertyNames = entry.OriginalValues.PropertyNames
                .Where(pn => entry.OriginalValues[pn] != entry.CurrentValues[pn])
                .ToList();

            // 2. collect values of changed properties
            Dictionary<string, object> changedValues = changedPropertyNames
                .ToDictionary(pn => pn, pn => entry.CurrentValues[pn]);

            // 3. reload values from database
            entry.Reload();

            // 4. resave only the changes values
            foreach (var changedValue in changedValues)
            {
                var property = entry.Property(changedValue.Key);
                property.CurrentValue = changedValue.Value;
                property.IsModified = true;
            }
        }
    }
}