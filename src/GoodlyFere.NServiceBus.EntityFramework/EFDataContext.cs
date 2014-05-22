#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="EFDataContext.cs">
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

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework
{
    public class EFDataContext : BaseDataContext
    {
        #region Constants and Fields

        private readonly EFDbContext _dbContext;

        #endregion

        #region Constructors and Destructors

        public EFDataContext()
        {
            _dbContext = new EFDbContext();
        }

        #endregion

        #region Public Methods

        public override void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }

        #endregion

        #region Methods

        protected override IRepository<T> GetRepository<T>()
        {
            Type repoType = typeof(EFRepository<>).MakeGenericType(typeof(T));
            return (IRepository<T>)Activator.CreateInstance(repoType, _dbContext);
        }

        #endregion
    }
}