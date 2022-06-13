using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;
using MicroOrm.Dapper.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace GenericCRUD
{
    public class GenericCrudLogic<T> : IGenericCrudLogic<T> where T : class
    {
        public Dictionary<string, Validator<T>> Validators { get; set; } = new();
        protected readonly IDapperRepository<T> _repo;


        public GenericCrudLogic(IDapperRepository<T> repo)
        {
            _repo = repo;
            SetupDefaultValidators();
        }

        private void SetupDefaultValidators()
        {
            #region Create
            
            var create = Validators[nameof(Create)] = new Validator<T>();
            
            create.ParameterValidation = (CrudParam<T> param, ref List<Exception> errors) =>
                IsNotNull(param, ref errors) && // ** If it's null, it's an API mistake. Shouldn't return to client.
                IsNotNull(param.Requester, ref errors) &&
                IsNotNull(param.Entity, ref errors);
            
            create.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            
            create.AuthorityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;
            
            #endregion

            #region Read
            
            var read = Validators[nameof(GetById)] = new Validator<T>();

            read.ParameterValidation = (CrudParam<T> param, ref List<Exception> errors) =>
                IsNotNull(param, ref errors) &&
                IsNotNull(param.Requester, ref errors) &&
                IsNotNullEmpty(param.EntityIds, ref errors);
            
            read.EntityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;
            
            read.AuthorityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;
            
            #endregion
            
            #region Update
            
            var update = Validators[nameof(Update)] = new Validator<T>();
            
            update.ParameterValidation = (CrudParam<T> param, ref List<Exception> errors) =>
                IsNotNull(param, ref errors) &&
                IsNotNull(param.Requester, ref errors) &&
                IsNotNull(param.Entity, ref errors);
                // ** Should I null check param.Entity.Id?
            
            update.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            
            update.AuthorityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;
            
            #endregion

            #region PartialUpdate
            
            var partial = Validators[nameof(PartialUpdate)] = new Validator<T>();
            
            partial.ParameterValidation = (CrudParam<T> param, ref List<Exception> errors) =>
                IsNotNull(param, ref errors) &&
                IsNotNull(param.Requester, ref errors) &&
                IsNotNull(param.Entity, ref errors) &&
                IsNotNull(param.Patch, ref errors);
            // ** Should I null check param.Entity.Id?
            
            partial.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            
            partial.AuthorityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;
            
            #endregion

            #region Delete
            
            var delete = Validators[nameof(Delete)] = new Validator<T>();
            delete.ParameterValidation = (CrudParam<T> param, ref List<Exception> errors) =>
                IsNotNull(param, ref errors) &&
                IsNotNull(param.Requester, ref errors) &&
                IsNotNullEmpty(param.EntityIds, ref errors);

            delete.EntityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;

            delete.AuthorityValidation = (CrudParam<T> param, ref List<Exception> errors) => true;

            #endregion
        }
        
        
        protected bool IsNotNull<J>(J item, ref List<Exception> errors)
        {
            if (item != null)
                return true;
            
            errors.Add(new ArgumentNullException(item.GetType().Name));
            return false;
        }
        protected bool IsNotNullEmpty<J>(IEnumerable<J> items, ref List<Exception> errors)
        {
            if (items != null && items.Count() > 0)
                return true;

            errors.Add(new ArgumentException("Argument can't be null or empty.", items.GetType().Name));

            return false;
        }
        protected bool IsInRange(ref List<Exception> errors, int integer, int inclusiveMin = int.MinValue, int inclusiveMax = int.MaxValue)
        {
            if (integer >= inclusiveMin || integer <= inclusiveMax)
                return true;

            errors.Add(new ArgumentException($"{nameof(integer)} ({integer}) isn't inclusively between {inclusiveMin} and {inclusiveMax}.", nameof(integer)));

            return false;
        }
        
        
        
        public ApiResult<int?> Create(CrudParam<T> param)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<int?> Create(CrudParam<T> param, T canvas)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<IEnumerable<T>> GetById(CrudParam<T> param)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<bool?> Update(CrudParam<T> param)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<bool?> PartialUpdate(CrudParam<T> param)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<bool?> Delete(CrudParam<T> param)
        {
            throw new System.NotImplementedException();
        }
    }
}