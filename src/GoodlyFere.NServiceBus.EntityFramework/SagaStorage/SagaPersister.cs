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
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using NServiceBus.Saga;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SagaStorage
{
    public class SagaPersister : ISagaPersister
    {
        private readonly ISagaDbContext _dbContext;

        public SagaPersister(IDbContextProvider dbContextProvider)
        {
            if (dbContextProvider == null)
            {
                throw new ArgumentNullException("dbContextProvider");
            }

            _dbContext = dbContextProvider.GetSagaDbContext();
        }

        public void Complete(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            try
            {
                DbEntityEntry entry = _dbContext.Entry(saga);
                DbSet set = _dbContext.SagaSet(saga.GetType());

                if (entry.State == EntityState.Detached)
                {
                    set.Attach(saga);
                }

                set.Remove(saga);

                _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                // don't do anything, if we couldn't delete because it doesn't exist, that's OK
            }
        }

        public TSagaData Get<TSagaData>(Guid sagaId) where TSagaData : IContainSagaData
        {
            if (sagaId == Guid.Empty)
            {
                throw new ArgumentException("sagaId cannot be empty.", "sagaId");
            }

            object result = _dbContext.SagaSet(typeof(TSagaData)).Find(sagaId);
            return (TSagaData)(result ?? default(TSagaData));
        }

        public TSagaData Get<TSagaData>(string propertyName, object propertyValue) where TSagaData : IContainSagaData
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            ParameterExpression param = Expression.Parameter(typeof(TSagaData), "sagaData");
            Expression<Func<TSagaData, bool>> filter = Expression.Lambda<Func<TSagaData, bool>>(
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.Property(param, propertyName),
                    Expression.Constant(propertyValue)),
                param);

            IQueryable setQueryable = _dbContext.SagaSet(typeof(TSagaData)).AsQueryable();
            IQueryable result = setQueryable
                .Provider
                .CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        "Where",
                        new[] { typeof(TSagaData) },
                        setQueryable.Expression,
                        Expression.Quote(filter)));

            IEnumerator enumerator = result.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return (TSagaData)enumerator.Current;
            }

            return default(TSagaData);
        }

        public void Save(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            _dbContext.SagaSet(saga.GetType()).Add(saga);
            _dbContext.SaveChanges();
        }

        public void Update(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            using (DbContextTransaction transaction = _dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    object existingEnt = _dbContext.SagaSet(saga.GetType()).Find(saga.Id);
                    if (existingEnt == null)
                    {
                        throw new Exception(string.Format("Could not find saga with ID {0}", saga.Id));
                    }

                    DbEntityEntry entry = _dbContext.Entry(existingEnt);
                    entry.CurrentValues.SetValues(saga);
                    entry.State = EntityState.Modified;
                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}