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
using System.Linq;
using GoodlyFere.Data;
using GoodlyFere.NServiceBus.EntityFramework.Criteria;
using GoodlyFere.NServiceBus.EntityFramework.Model;
using NServiceBus.Saga;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public class EFSagaPersister : IPersistSagas
    {
        #region Constants and Fields

        private readonly IDataContext _dataContext;

        #endregion

        #region Constructors and Destructors

        public EFSagaPersister(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        #endregion

        #region Public Methods

        public void Complete(IContainSagaData saga)
        {
            var concreteSagaData = saga as SagaData;
            concreteSagaData.IsCompleted = true;

            _dataContext.Update(concreteSagaData);
        }

        public T Get<T>(Guid sagaId) where T : IContainSagaData
        {
            return _dataContext.FindById<T>(sagaId);
        }

        public T Get<T>(string property, object value) where T : IContainSagaData
        {
            return _dataContext.FindOne(new SagaCriteria<T>(property, value));
        }

        public void Save(IContainSagaData saga)
        {
            var concreteSagaData = saga as SagaData;

            if (saga.Id == Guid.Empty)
            {
                saga.Id = Guid.NewGuid();
            }

            _dataContext.Create(concreteSagaData);
        }

        public void Update(IContainSagaData saga)
        {
            var concreteSagaData = saga as SagaData;
            _dataContext.Update(concreteSagaData);
        }

        #endregion
    }
}