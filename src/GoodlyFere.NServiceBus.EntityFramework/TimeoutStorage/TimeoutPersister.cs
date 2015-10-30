#region License

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeoutPersister.cs">
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
using System.Collections.Generic;
using System.Data;
using GoodlyFere.NServiceBus.EntityFramework.Support;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using GoodlyFere.NServiceBus.EntityFramework.SharedDbContext;
using NServiceBus;
using NServiceBus.Timeout.Core;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    public class TimeoutPersister : IPersistTimeouts
    {
        private readonly ITimeoutDbContext _dbContext;

        public TimeoutPersister(IDbContextProvider dbContextProvider)
        {
            if (dbContextProvider == null)
            {
                throw new ArgumentNullException("dbContextProvider");
            }

            _dbContext = dbContextProvider.GetTimeoutDbContext();
        }

        public string EndpointName { get; set; }

        public void Add(TimeoutData timeout)
        {
            if (timeout == null)
            {
                throw new ArgumentNullException("timeout");
            }

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

            _dbContext.Timeouts.Add(timeoutEntity);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            DateTime now = DateTime.UtcNow;

            List<TimeoutDataEntity> matchingTimeouts = _dbContext.Timeouts
                .Where(
                    t => t.Endpoint == EndpointName
                         && t.Time >= startSlice
                         && t.Time <= now)
                .OrderBy(t => t.Time)
                .ToList();

            List<Tuple<string, DateTime>> chunks = matchingTimeouts
                .Select(t => new Tuple<string, DateTime>(t.Id.ToString(), t.Time))
                .ToList();

            TimeoutDataEntity startOfNextChunk = _dbContext.Timeouts
                .Where(t => t.Endpoint == EndpointName && t.Time > now)
                .OrderBy(t => t.Time)
                .Take(1)
                .SingleOrDefault();

            nextTimeToRunQuery = startOfNextChunk != null
                ? startOfNextChunk.Time
                : DateTime.UtcNow.AddMinutes(10);

            return chunks;
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            if (sagaId == Guid.Empty)
            {
                throw new ArgumentException("sagaId parameter cannot be empty.", "sagaId");
            }

            var toDelete = _dbContext.Timeouts.Where(t => t.SagaId == sagaId);
            _dbContext.Timeouts.RemoveRange(toDelete);

            _dbContext.SaveChanges();
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            TimeoutDataEntity entity = _dbContext.Timeouts.Find(Guid.Parse(timeoutId));

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

            _dbContext.Timeouts.Remove(entity);
            _dbContext.SaveChanges();

            return true;
        }
    }
}