#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="EFTimeoutPersister.cs">
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
using System.Linq;

#endregion

public class EFTimeoutPersister : IPersistTimeouts
{
    #region Constants and Fields

    private readonly IDataContext _dataContext;

    #endregion

    #region Constructors and Destructors

    public EFTimeoutPersister(IDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    #endregion

    #region Public Methods

    public void Add(TimeoutData timeout)
    {
        if (string.IsNullOrWhiteSpace(timeout.Id))
        {
            timeout.Id = Guid.NewGuid().ToString();
        }

        TimeoutDataEntity timeoutEntity = new TimeoutDataEntity
            {
                CorrelationId = timeout.CorrelationId,
                Destination = timeout.Destination.ToString(),
                Headers = JsonConvert.SerializeObject(timeout.Headers),
                Id = timeout.Id,
                OwningTimeoutManager = timeout.OwningTimeoutManager,
                SagaId = timeout.SagaId,
                State = timeout.State,
                Time = timeout.Time
            };

        _dataContext.Create(timeoutEntity);
    }

    public List<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
    {
        DateTime now = DateTime.UtcNow;
        IList<TimeoutDataEntity> matchingTimeouts =
            _dataContext.Find(new NextChunkTimeout(startSlice, now, Configure.EndpointName));

        TimeoutDataEntity nextTimeout = _dataContext.FindOne(
            new FutureTimeout(now, Configure.EndpointName), t => t.Time);
        nextTimeToRunQuery = nextTimeout == null ? now.AddMinutes(10) : nextTimeout.Time;

        return matchingTimeouts
            .Select(t => new Tuple<string, DateTime>(t.Id, t.Time))
            .ToList();
    }

    public void RemoveTimeoutBy(Guid sagaId)
    {
        TimeoutDataEntity[] entitiesToDeletes = _dataContext.Find(new BelongsToSaga(sagaId)).ToArray();

        foreach (var entity in entitiesToDeletes)
        {
            _dataContext.Delete(entity);
        }
    }

    public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
    {
        var entity = _dataContext.FindById<TimeoutDataEntity>(timeoutId);

        if (entity == null)
        {
            timeoutData = null;
            return false;
        }

        timeoutData = new TimeoutData
            {
                CorrelationId = entity.CorrelationId,
                Destination = Address.Parse(entity.Destination),
                Headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(entity.Headers),
                Id = entity.Id,
                OwningTimeoutManager = entity.OwningTimeoutManager,
                SagaId = entity.SagaId,
                State = entity.State,
                Time = entity.Time
            };

        try
        {
            _dataContext.Delete(entity);
            return true;
        }
        catch (Exception)
        {
            timeoutData = null;
            return false;
        }
    }

    #endregion
}