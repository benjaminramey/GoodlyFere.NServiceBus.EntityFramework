#region License

// ------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValidationExtensions.cs">
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Validation;

#endregion

namespace GoodlyFere.NServiceBus.EntityFramework.Model
{
    public static class ValidationExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Executes validation and returns true if valid and false if not.
        /// </summary>
        /// <typeparam name="T">Type of object to validate</typeparam>
        /// <param name="objToValidate">Object to validate</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool IsValid<T>(this T objToValidate)
        {
            try
            {
                objToValidate.Validate();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Performs validation and throws a ValidationException if the object is invalid.
        /// </summary>
        /// <typeparam name="T">Type of object to validate</typeparam>
        /// <param name="objToValidate">Object to validate</param>
        public static void Validate<T>(this T objToValidate)
        {
            Validator<T> validator = ValidationFactory.CreateValidator<T>();
            ValidationResults results = validator.Validate(objToValidate);

            if (results.IsValid)
            {
                return;
            }

            string msg = string.Join("\n", results.Select(r => r.Message));
            msg = string.Format("Invalid object! {0}", msg);

            throw new ValidationException(msg);
        }

        #endregion
    }
}