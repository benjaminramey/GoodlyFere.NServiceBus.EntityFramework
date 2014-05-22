#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="IDataContext.cs">
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

public interface IDataContext : IDisposable
{
    #region Public Methods

    void Create<T>(T newObject);

    void Delete<T>(T objectToDelete);

    IList<T> Find<T>(ICriteria<T> criteria);

    T FindById<T>(object id);

    T FindOne<T>(ICriteria<T> criteria);

    T FindOne<T, TSortKey>(ICriteria<T> criteria, Expression<Func<T, TSortKey>> ordering, bool desc = false);

    IList<T> GetAll<T>();

    T GetOne<T>();

    void LoadChildren<T>(T obj, string propertyName);

    void LoadParent<T>(T obj, string propertyName);

    void Update<T>(T newObject);

    #endregion
}