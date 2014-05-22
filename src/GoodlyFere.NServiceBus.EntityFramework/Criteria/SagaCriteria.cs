#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="SagaCriteria.cs">
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
using System.Linq.Expressions;
using System.Reflection;
using GoodlyFere.Criteria;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Criteria
{
    public class SagaCriteria<T> : BinaryCriteria<T>
    {
        #region Constants and Fields

        private readonly string _property;
        private readonly object _value;

        #endregion

        #region Constructors and Destructors

        public SagaCriteria(string property, object value)
        {
            _property = property;
            _value = value;
        }

        #endregion

        #region Public Properties

        public override Expression<Func<T, bool>> Satisfier
        {
            get
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(_property);
                Type propertyType = propertyInfo.PropertyType;

                ParameterExpression pe = Expression.Parameter(typeof(T), "data");
                MemberExpression me = Expression.MakeMemberAccess(pe, propertyInfo);

                if (propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    PropertyInfo valuePropInfo = propertyType.GetProperty("Value");
                    me = Expression.MakeMemberAccess(me, valuePropInfo);
                }

                ConstantExpression ce = Expression.Constant(_value);
                BinaryExpression be = Expression.Equal(me, ce);

                return Expression.Lambda<Func<T, bool>>(be, new[] { pe });
            }
        }

        #endregion
    }
}