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

        public TimeoutPersister(INServiceBusDbContextFactory dbContextFactory)
        {
            if (dbContextFactory == null)
            {
                throw new ArgumentNullException("dbContextFactory");
            }

            _dbContextFactory = dbContextFactory;
        }

        public string EndpointName { get; set; }

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
                List<TimeoutDataEntity> matchingTimeouts = dbc.Timeouts
                    .Where(
                        t => t.Endpoint == EndpointName
                             && t.Time >= startSlice
                             && t.Time <= now)
                    .OrderBy(t => t.Time)
                    .ToList();

                List<Tuple<string, DateTime>> chunks = matchingTimeouts
                    .Select(t => new Tuple<string, DateTime>(t.Id.ToString(), t.Time))
                    .ToList();

                TimeoutDataEntity startOfNextChunk = dbc.Timeouts
                    .Where(t => t.Endpoint == EndpointName && t.Time > now)
                    .OrderBy(t => t.Time)
                    .Take(1)
                    .SingleOrDefault();

                nextTimeToRunQuery = startOfNextChunk != null
                    ? startOfNextChunk.Time
                    : DateTime.UtcNow.AddMinutes(10);

                return chunks;
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
                    TimeoutDataEntity entity = dbc.Timeouts.Find(Guid.Parse(timeoutId));

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