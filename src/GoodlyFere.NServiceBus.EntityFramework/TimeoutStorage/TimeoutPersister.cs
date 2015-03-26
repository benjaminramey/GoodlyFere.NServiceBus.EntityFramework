#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using GoodlyFere.NServiceBus.EntityFramework.Support;
using NServiceBus.Timeout.Core;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    public class TimeoutPersister : IPersistTimeouts
    {
        private readonly NServiceBusDbContext _dbContext;
        private readonly string _endpointName;

        public TimeoutPersister(NServiceBusDbContext dbContext, string endpointName)
        {
            _endpointName = endpointName;
            _dbContext = dbContext;
        }

        public void Add(TimeoutData timeout)
        {
            Guid timeoutId = CombGuid.NewGuid();

            TimeoutDataEntity timeoutEntity = new TimeoutDataEntity
            {
                Destination = timeout.Destination.ToAddressString(),
                Endpoint = timeout.OwningTimeoutManager,
                Headers = timeout.Headers.ToDictionaryString(),
                Id = timeoutId,
                SagaId = timeout.SagaId,
                State = timeout.State,
                Time = timeout.Time
            };

            _dbContext.Timeouts.Add(timeoutEntity);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            DateTime now = DateTime.UtcNow;

            var matchingTimeouts = _dbContext.Timeouts
                .Where(
                    t => t.Endpoint == _endpointName
                         && t.Time >= startSlice
                         && t.Time <= now)
                .OrderBy(t => t.Time)
                .Select(t => new Tuple<string, DateTime>(t.Id.ToString(), t.Time))
                .ToList();

            var startOfNextChunk = _dbContext.Timeouts
                .Where(t => t.Endpoint == _endpointName)
                .Where(t => t.Time > now)
                .OrderBy(t => t.Time)
                .Take(1)
                .SingleOrDefault();

            nextTimeToRunQuery = startOfNextChunk != null ? startOfNextChunk.Time : DateTime.UtcNow.AddMinutes(10);

            return matchingTimeouts;
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            _dbContext.Timeouts.Where(t => t.SagaId == sagaId).Delete();
            _dbContext.SaveChanges();
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            TimeoutDataEntity entity = _dbContext.Timeouts.Find(timeoutId);

            if (entity == null)
            {
                timeoutData = null;
                return false;
            }

            timeoutData = new TimeoutData
            {
                Destination = entity.Destination.ToAddress(),
                Headers = entity.Headers.ToDictionary(),
                Id = entity.Id.ToString(),
                OwningTimeoutManager = entity.Endpoint,
                SagaId = entity.SagaId,
                State = entity.State,
                Time = entity.Time
            };

            _dbContext.Timeouts.Remove(entity);
            _dbContext.SaveChanges();

            return true;
        }
    }
}