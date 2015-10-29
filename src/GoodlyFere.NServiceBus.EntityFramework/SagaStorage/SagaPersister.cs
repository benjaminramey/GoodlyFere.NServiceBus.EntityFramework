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
using GoodlyFere.NServiceBus.EntityFramework.Support;
using NServiceBus.Saga;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SagaStorage
{
    public class SagaPersister : ISagaPersister
    {
        private readonly INServiceBusDbContextFactory _dbContextFactory;

        public SagaPersister(INServiceBusDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Complete(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            using (ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext())
            {
                try
                {
                    DbEntityEntry entry = dbc.Entry(saga);
                    DbSet set = dbc.SagaSet(saga.GetType());

                    if (entry.State == EntityState.Detached)
                    {
                        set.Attach(saga);
                    }

                    set.Remove(saga);

                    dbc.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // don't do anything, if we couldn't delete because it doesn't exist, that's OK
                }
            }
        }

        public TSagaData Get<TSagaData>(Guid sagaId) where TSagaData : IContainSagaData
        {
            if (sagaId == Guid.Empty)
            {
                throw new ArgumentException("sagaId cannot be empty.", "sagaId");
            }

            using (ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext())
            {
                object result = dbc.SagaSet(typeof(TSagaData)).Find(sagaId);
                return (TSagaData)(result ?? default(TSagaData));
            }
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

            using (ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext())
            {
                IQueryable setQueryable = dbc.SagaSet(typeof(TSagaData)).AsQueryable();
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
        }

        public void Save(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            using (ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext())
            {
                dbc.SagaSet(saga.GetType()).Add(saga);
                dbc.SaveChanges();
            }
        }

        public void Update(IContainSagaData saga)
        {
            if (saga == null)
            {
                throw new ArgumentNullException("saga");
            }

            using (ISagaDbContext dbc = _dbContextFactory.CreateSagaDbContext())
            {
                object existingEnt = dbc.SagaSet(saga.GetType()).Find(saga.Id);
                if (existingEnt == null)
                {
                    throw new Exception(string.Format("Could not find saga with ID {0}", saga.Id));
                }

                var entry = dbc.Entry(existingEnt);
                entry.CurrentValues.SetValues(saga);
                entry.State = EntityState.Modified;
                dbc.SaveChanges();
            }
        }
    }
}