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
using GoodlyFere.NServiceBus.EntityFramework.Support;
using System.Linq;
using GoodlyFere.NServiceBus.EntityFramework.Interfaces;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Timeout.Core;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.TimeoutStorage
{
    public class TimeoutPersister : IPersistTimeouts, IPersistTimeoutsV2
    {
        private static readonly ILog Logger = LogManager.GetLogger<TimeoutPersister>();
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
            Logger.Debug("Adding timeout");

            if (timeout == null)
            {
                Logger.Debug("Timeout is null! Throwing");
                throw new ArgumentNullException("timeout");
            }

            Guid timeoutId = CombGuid.NewGuid();
            Logger.DebugFormat("Created new comb guid for timeout ID {0}", timeoutId);
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

            using (ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                Logger.Debug("Saving timeout entity");
                dbc.Timeouts.Add(timeoutEntity);
                dbc.SaveChanges();
            }
        }

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            Logger.DebugFormat("Getting next chuch with start slice {0}", startSlice);
            DateTime now = DateTime.UtcNow;

            using (ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                Logger.Debug("Querying for matching timeouts within time slice");
                List<TimeoutDataEntity> matchingTimeouts = dbc.Timeouts
                    .Where(
                        t => t.Endpoint == EndpointName
                             && t.Time >= startSlice
                             && t.Time <= now)
                    .OrderBy(t => t.Time)
                    .ToList();

                Logger.DebugFormat("{0} timeout(s) found.", matchingTimeouts.Count);

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

                Logger.DebugFormat("Next time to run query calculated to be {0}.", nextTimeToRunQuery);

                return chunks;
            }
        }

        public TimeoutData Peek(string timeoutId)
        {
            Logger.DebugFormat("Peeking for timeout with id {0}", timeoutId);

            using (ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                TimeoutDataEntity entity = dbc.Timeouts.Find(Guid.Parse(timeoutId));
                Logger.DebugFormat("Timeout found? {0}", entity != null);             
                return MapToTimeoutData(entity);
            }
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            Logger.DebugFormat("Removing timeout for saga ID {0}", sagaId);

            if (sagaId == Guid.Empty)
            {
                Logger.Debug("sageId is empty! Throwing");
                throw new ArgumentException("sagaId parameter cannot be empty.", "sagaId");
            }

            using (ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                IQueryable<TimeoutDataEntity> toDelete = dbc.Timeouts.Where(t => t.SagaId == sagaId);
                Logger.DebugFormat("{0} timeout(s) found for saga.", toDelete.Count());
                dbc.Timeouts.RemoveRange(toDelete);

                dbc.SaveChanges();
            }
        }

        public bool TryRemove(string timeoutId)
        {
            Logger.DebugFormat("Trying to remove timeout with Id {0}.", timeoutId);

            using (ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                Guid timeoutGuid = Guid.Parse(timeoutId);
                TimeoutDataEntity entity = dbc.Timeouts.Find(timeoutGuid);

                if (entity == null)
                {
                    Logger.Debug("Didn't find timeout!");
                    return false;
                }

                Logger.Debug("Found timeout, removing.");
                dbc.Timeouts.Remove(entity);
                dbc.SaveChanges();
            }

            return true;
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            Logger.DebugFormat("Trying to remove timeout with Id {0} and returning timeout data.", timeoutId);

            using (ITimeoutDbContext dbc = _dbContextFactory.CreateTimeoutDbContext())
            {
                TimeoutDataEntity entity = dbc.Timeouts.Find(Guid.Parse(timeoutId));

                if (entity == null)
                {
                    Logger.Debug("Didn't find timeout!");
                    timeoutData = null;
                    return false;
                }

                Logger.Debug("Found timeout, removing.");
                timeoutData = MapToTimeoutData(entity);
                dbc.Timeouts.Remove(entity);
                dbc.SaveChanges();
            }

            return true;
        }

        private TimeoutData MapToTimeoutData(TimeoutDataEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            TimeoutData timeoutData = new TimeoutData
            {
                Destination = Address.Parse(entity.Destination),
                Headers = entity.Headers.ToDictionary(),
                Id = entity.Id.ToString(),
                OwningTimeoutManager = entity.Endpoint,
                SagaId = entity.SagaId,
                State = entity.State,
                Time = entity.Time
            };

            return timeoutData;
        }
    }
}