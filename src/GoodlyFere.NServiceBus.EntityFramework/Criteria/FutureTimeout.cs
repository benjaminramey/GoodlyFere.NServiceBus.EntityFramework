#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="FutureTimeout.cs">
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
using GoodlyFere.Criteria;
using GoodlyFere.NServiceBus.EntityFramework.Model;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Criteria
{
    public class FutureTimeout : BinaryCriteria<TimeoutDataEntity>
    {
        #region Constants and Fields

        private readonly string _endpoint;
        private readonly DateTime _now;

        #endregion

        #region Constructors and Destructors

        public FutureTimeout(DateTime now, string endpoint)
        {
            _now = now;
            _endpoint = endpoint;
        }

        #endregion

        #region Public Properties

        public override Expression<Func<TimeoutDataEntity, bool>> Satisfier
        {
            get
            {
                return t => t.OwningTimeoutManager == _endpoint
                            && t.Time > _now;
            }
        }

        #endregion
    }
}