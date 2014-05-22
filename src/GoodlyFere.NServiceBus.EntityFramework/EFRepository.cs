#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="EFRepository.cs">
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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using GoodlyFere.Criteria;
using GoodlyFere.Data;
using GoodlyFere.NServiceBus.EntityFramework.Model;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public class EFRepository<T> : IRepository<T>
        where T : class
    {
        #region Constructors and Destructors

        public EFRepository(DbContext dbContext)
        {
            DBContext = dbContext;
        }

        #endregion

        #region Properties

        protected DbContext DBContext { get; private set; }

        #endregion

        #region Public Methods

        public virtual void Add(T newObject)
        {
            try
            {
                newObject.Validate();

                DBContext.Set<T>().Add(newObject);
                DBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                this.Log().Error(() => "Could not update object.", ex);
                throw;
            }
        }

        public void Dispose()
        {
        }

        public virtual IList<T> Find(ICriteria<T> criteria)
        {
            return DBContext.Set<T>().Where(criteria.Satisfier).ToList();
        }

        public T FindById(object id)
        {
            return DBContext.Set<T>().Find(id);
        }

        public virtual T FindOne(ICriteria<T> criteria)
        {
            T result = DBContext.Set<T>().FirstOrDefault(criteria.Satisfier);

            return result;
        }

        public virtual T FindOne()
        {
            throw new NotImplementedException();
        }

        public T FindOne<TSortKey>(ICriteria<T> criteria, Expression<Func<T, TSortKey>> ordering, bool desc = false)
        {
            if (desc)
            {
                return DBContext.Set<T>().Where(criteria.Satisfier).OrderByDescending(ordering).FirstOrDefault();
            }

            return DBContext.Set<T>().Where(criteria.Satisfier).OrderBy(ordering).FirstOrDefault();
        }

        public virtual IList<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public void LoadChildren(T obj, string propertyName)
        {
            DBContext.Entry(obj).Collection(propertyName).Load();
        }

        public void LoadParent(T obj, string propertyName)
        {
            DBContext.Entry(obj).Reference(propertyName).Load();
        }

        public virtual void Remove(T objectToDelete)
        {
            DbEntityEntry<T> entity = DBContext.Entry(objectToDelete);

            if (entity.State == EntityState.Detached)
            {
                DBContext.Set<T>().Attach(objectToDelete);
            }

            DBContext.Set<T>().Remove(objectToDelete);
            DBContext.SaveChanges();
        }

        public virtual void Update(T objectToUpdate)
        {
            try
            {
                objectToUpdate.Validate();

                DbEntityEntry<T> entity = DBContext.Entry(objectToUpdate);

                if (entity.State == EntityState.Detached)
                {
                    entity.State = EntityState.Modified;
                }
                else if (entity.State == EntityState.Unchanged)
                {
                    throw new Exception("nothing to change!");
                }

                DBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                this.Log().Error(() => "Could not update object.", ex);
                throw;
            }
        }

        #endregion
    }
}