#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EntityFramework.Extensions;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.Support;
using NServiceBus;
using NServiceBus.Timeout.Core;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    public class TimeoutPersister : IPersistTimeouts
    {
        private readonly INServiceBusDbContextFactory _dbContextFactory;
        private readonly string _endpointName;

        public TimeoutPersister(string endpointName, INServiceBusDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _endpointName = endpointName;
        }

        public void Add(TimeoutData timeout)
        {
            Guid timeoutId = CombGuid.NewGuid();
            TimeoutDataEntity timeoutEntity = new TimeoutDataEntity
            {
                Destination = timeout.Destination.ToString(),
                Endpoint = timeout.OwningTimeoutManager,
                Headers = timeout.Headers.ToDictionaryString(),
                Id = timeoutId,
                SagaId = timeout.SagaId,
                State = timeout.State,
                Time = timeout.Time
            };

            using (var dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        dbc.Timeouts.Add(timeoutEntity);
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

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            DateTime now = DateTime.UtcNow;

            using (var dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                var matchingTimeouts = dbc.Timeouts
                    .Where(
                        t => t.Endpoint == _endpointName
                             && t.Time >= startSlice
                             && t.Time <= now)
                    .OrderBy(t => t.Time)
                    .Select(t => new Tuple<string, DateTime>(t.Id.ToString(), t.Time))
                    .ToList();

                var startOfNextChunk = dbc.Timeouts
                    .Where(t => t.Endpoint == _endpointName)
                    .Where(t => t.Time > now)
                    .OrderBy(t => t.Time)
                    .Take(1)
                    .SingleOrDefault();

                nextTimeToRunQuery = startOfNextChunk != null
                    ? startOfNextChunk.Time
                    : DateTime.UtcNow.AddMinutes(10);

                return matchingTimeouts;
            }
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            using (var dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    dbc.Timeouts.Where(t => t.SagaId == sagaId).Delete();

                    dbc.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            using (var dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                using (var transaction = dbc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    TimeoutDataEntity entity = dbc.Timeouts.Find(timeoutId);

                    if (entity == null)
                    {
                        timeoutData = null;
                        return false;
                    }

                    timeoutData = new TimeoutData
                    {
                        Destination = Address.Parse(entity.Destination),
                        Headers = entity.Headers.ToDictionary(),
                        Id = entity.Id.ToString(),
                        OwningTimeoutManager = entity.Endpoint,
                        SagaId = entity.SagaId,
                        State = entity.State,
                        Time = entity.Time
                    };

                    dbc.Timeouts.Remove(entity);
                    dbc.SaveChanges();
                    transaction.Commit();

                    return true;
                }
            }
        }
    }
}