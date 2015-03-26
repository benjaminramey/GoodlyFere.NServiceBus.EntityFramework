#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="EFSagaPersister.cs">
//  GoodlyFere.NServiceBus.EntityFramework
//  
//  Copyright (C) 2014 
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//  
//  http://www.gnu.org/licenses/lgpl-2.1-standalone.html
//  
//  You can contact me at ben.ramey@gmail.com.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

#endregion

#region Usings

using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using NServiceBus.Saga;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.SagaStorage
{
    public class SagaPersister : ISagaPersister
    {
        private readonly NServiceBusDbContextFactory _dbContextFactory;

        public SagaPersister()
        {
            _dbContextFactory = new NServiceBusDbContextFactory();
        }

        public void Save(IContainSagaData saga)
        {
            saga.Id = Support.CombGuid.NewGuid();

            using (var dbc = _dbContextFactory.Create())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dbc.Set(saga.GetType()).Add(saga);
                    dbc.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        public void Update(IContainSagaData saga)
        {
            using (var dbc = _dbContextFactory.Create())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    object existingEnt = dbc.Set(saga.GetType()).Find(saga.Id);
                    if (existingEnt == null)
                    {
                        throw new Exception(string.Format("Could not find saga with ID {0}", saga.Id));
                    }

                    var entry = dbc.Entry(existingEnt);
                    entry.CurrentValues.SetValues(saga);
                    entry.State = EntityState.Modified;
                    dbc.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        public TSagaData Get<TSagaData>(Guid sagaId) where TSagaData : IContainSagaData
        {
            using (var dbc = _dbContextFactory.Create())
            {
                object result = dbc.Set(typeof(TSagaData)).Find(sagaId);
                return (TSagaData)(result ?? default(TSagaData));
            }
        }

        public TSagaData Get<TSagaData>(string propertyName, object propertyValue) where TSagaData : IContainSagaData
        {
            ParameterExpression param = Expression.Parameter(typeof(object));
            Expression<Func<TSagaData, bool>> filter = Expression.Lambda<Func<TSagaData, bool>>(
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.Property(param, propertyName),
                    Expression.Constant(propertyValue)),
                param);

            using (var dbc = _dbContextFactory.Create())
            {
                IQueryable result = dbc.Set(typeof(TSagaData)).Where(filter.ToString());

                if (result.Any())
                {
                    return (TSagaData)result.Take(1);
                }

                return default(TSagaData);
            }
        }

        public void Complete(IContainSagaData saga)
        {
            using (var dbc = _dbContextFactory.Create())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        dbc.Set(saga.GetType()).Remove(saga);

                        dbc.SaveChanges();
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
}