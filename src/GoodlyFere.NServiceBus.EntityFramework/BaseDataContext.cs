#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="BaseDataContext.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GoodlyFere.Criteria;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public abstract class BaseDataContext : IDataContext
    {
        #region Public Methods

        public virtual void Create<T>(T newObject)
        {
            using (var repo = GetRepository<T>())
            {
                repo.Add(newObject);
            }
        }

        public void Delete<T>(T objectToDelete)
        {
            using (var repo = GetRepository<T>())
            {
                repo.Remove(objectToDelete);
            }
        }

        public abstract void Dispose();

        public virtual IList<T> Find<T>(ICriteria<T> criteria)
        {
            using (var repo = GetRepository<T>())
            {
                return repo.Find(criteria);
            }
        }

        public T FindById<T>(object id)
        {
            using (var repo = GetRepository<T>())
            {
                return repo.FindById(id);
            }
        }

        public virtual T FindOne<T>(ICriteria<T> criteria)
        {
            using (var repo = GetRepository<T>())
            {
                return repo.FindOne(criteria);
            }
        }

        public T FindOne<T, TSortKey>(ICriteria<T> criteria, Expression<Func<T, TSortKey>> ordering, bool desc = false)
        {
            using (var repo = GetRepository<T>())
            {
                return repo.FindOne(criteria, ordering, desc);
            }
        }

        public virtual IList<T> GetAll<T>()
        {
            using (var repo = GetRepository<T>())
            {
                return repo.GetAll();
            }
        }

        public virtual T GetOne<T>()
        {
            using (var repo = GetRepository<T>())
            {
                return repo.FindOne();
            }
        }

        public void LoadChildren<T>(T obj, string propertyName)
        {
            using (var repo = GetRepository<T>())
            {
                repo.LoadChildren(obj, propertyName);
            }
        }

        public void LoadParent<T>(T obj, string propertyName)
        {
            using (var repo = GetRepository<T>())
            {
                repo.LoadParent(obj, propertyName);
            }
        }

        public virtual void Update<T>(T newObject)
        {
            using (var repo = GetRepository<T>())
            {
                repo.Update(newObject);
            }
        }

        #endregion

        #region Methods

        protected abstract IRepository<T> GetRepository<T>();

        #endregion
    }
}