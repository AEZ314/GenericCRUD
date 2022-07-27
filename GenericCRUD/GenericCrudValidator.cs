using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;
using FluentValidation;
using MicroOrm.Dapper.Repositories;

namespace GenericCRUD
{
    /// <summary>
    /// Encapsulates the 3 primitive ValidationDelegates. Each CRUD action has its own Validator. Each validator wraps 3
    /// ValidationDelegates.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class GenericCrudValidator<T> where T : class, IIdEntity
    {
        /// <summary>
        /// Validate whether the API code passed a valid CrudParam. Doesn't validate the API clients' inputs.
        /// </summary>
        public ValidationDelegate<T> ParameterValidation { get; set; } = (CrudParam<T> param, ref List<ValidationError> errors) => true;
        /// <summary>
        /// Validate the properties, indexes, Ids of the API clients' input. 
        /// </summary>
        public ValidationDelegate<T> EntityValidation { get; set; } = (CrudParam<T> param, ref List<ValidationError> errors) => true;
        /// <summary>
        /// Validate the authority of the API clients to perform the given action.
        /// </summary>
        public ValidationDelegate<T> AuthorityValidation { get; set; } = (CrudParam<T> param, ref List<ValidationError> errors) => true;

        public virtual bool Validate(CrudParam<T> param, ref List<ValidationError> errors)
        {
            var result = true;

            if (ParameterValidation != null && ParameterValidation(param, ref errors))
                result = false;
            if (EntityValidation != null && EntityValidation(param, ref errors))
                result = false;
            if (AuthorityValidation != null && AuthorityValidation(param, ref errors))
                result = false;

            return result;
        }
    }
}