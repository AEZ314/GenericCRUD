using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MicroOrm.Dapper.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace GenericCRUD
{
    public class GenericCrudLogic<T> : IGenericCrudLogic<T> where T : class, IIdEntity
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
            
            create.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) && 
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Entity, ref errors);
            
            create.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            
            create.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region Read
            
            var read = Validators[nameof(GetById)] = new Validator<T>();

            read.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNullEmpty(param.EntityIds, ref errors);
            
            read.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            read.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion
            
            #region Update
            
            var update = Validators[nameof(Update)] = new Validator<T>();
            
            update.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Entity, ref errors) &&
                IsNotNull(param.Entity.Id, ref errors);
            
            update.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            
            update.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region PartialUpdate
            
            var partial = Validators[nameof(PartialUpdate)] = new Validator<T>();
            
            partial.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNullEmpty(param.EntityIds, ref errors) &&
                IsNotNull(param.Patch, ref errors);
            
            partial.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            
            partial.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region Delete
            
            var delete = Validators[nameof(Delete)] = new Validator<T>();
            delete.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNullEmpty(param.EntityIds, ref errors);

            delete.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;

            delete.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;

            #endregion
        }
        
        // Set @throw true if API is responsible of error
        protected bool IsNotNull<J>(J item, ref List<ValidationError> errors, bool @throw = false)
        {
            if (item != null)
                return true;
            
            var error = new ValidationError(item.GetType().Name, "Argument can't be null.");
            errors.Add(error);
            
            if (@throw)
                throw new ArgumentNullException(item.GetType().Name);
            
            return false;
        }
        protected bool IsNotNullEmpty<J>(IEnumerable<J> items, ref List<ValidationError> errors, bool @throw = false)
        {
            if (items != null && items.Count() > 0)
                return true;
            
            var error = new ValidationError(items.GetType().Name, "Argument can't be null or empty.");
            errors.Add(error);
            
            if (@throw)
                throw new ArgumentException("Argument can't be null or empty.", items.GetType().Name);

            return false;
        }
        protected bool IsInRange(ref List<ValidationError> errors, int integer, int inclusiveMin = int.MinValue, int inclusiveMax = int.MaxValue, bool @throw = false)
        {
            if (integer >= inclusiveMin || integer <= inclusiveMax)
                return true;

            var error = new ValidationError(nameof(integer), $"{nameof(integer)} ({integer}) isn't inclusively between {inclusiveMin} and {inclusiveMax}.");
            errors.Add(error);
            
            if (@throw)
                throw new ArgumentException($"{nameof(integer)} ({integer}) isn't inclusively between {inclusiveMin} and {inclusiveMax}.", nameof(integer));

            return false;
        }
        
        
        
        public async Task<ApiResult<int?>> Create(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Create)].Validate(param, ref errors))
                return new ApiResult<int?>() { Result = null, Successful = false, Errors = errors };

            await _repo.InsertAsync(param.Entity);

            return new ApiResult<int?>(param.Entity.Id);
        }

        public async Task<ApiResult<IEnumerable<T>>> GetById(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(GetById)].Validate(param, ref errors))
                return new ApiResult<IEnumerable<T>>() { Result = null, Successful = false, Errors = errors };

            var entities = await _repo.FindAllAsync(x => param.EntityIds.Contains(x.Id));

            return new ApiResult<IEnumerable<T>>(entities);
        }

        // ** Are the Update/PartialUpdate Actions flexible and secure enough?
        
        public async Task<ApiResult<bool?>> Update(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Update)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };
            
            var success = await _repo.UpdateAsync(param.Entity);
            
            return new ApiResult<bool?>(success);
        }

        public async Task<ApiResult<bool?>> PartialUpdate(CrudParam<T> param)
        {
            var original = await _repo.FindByIdAsync(param.EntityIds.First());
            param.Patch.ApplyTo(original);
            param.Entity = original;
            return await Update(param);
        }

        public async Task<ApiResult<bool?>> Delete(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Delete)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };

            var success = await _repo.DeleteAsync(x => param.EntityIds.Contains(x.Id));

            return new ApiResult<bool?>(success);        
        }
    }
}