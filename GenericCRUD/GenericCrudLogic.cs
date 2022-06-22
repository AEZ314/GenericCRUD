using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using FluentValidation;
using MicroOrm.Dapper.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace GenericCRUD
{
    
    /// <summary>
    /// Default implementation for IGenericCrudLogic. You can add your custom logic or override default logic by inheriting this class.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class GenericCrudLogic<T> : IGenericCrudLogic<T> where T : class, IIdEntity
    {
        /// <summary>
        /// Maps CRUD methods' names to their respective validation methods. Default CRUD method implementations use
        /// nameof(Method) to get their validators. You can likewise override existing validators through this dictionary.
        /// </summary>
        public Dictionary<string, Validator<T>> Validators { get; set; } = new();
        /// <summary>
        /// Set this if you want to use fluent validation instead of DataAnnotations. 
        /// </summary>
        public AbstractValidator<T> FluentValidator { get; set; }
        /// <summary>
        /// Set this to configure DB Entity mappings. Eg: [Join] attributes. This is related to the inner workings of dapper-extensions lib.
        /// </summary>
        public Func<Expression<Func<T, bool>>, IDapperRepository<T>, Task<IEnumerable<T>>> DbReadChildSelector { get; set; } = (exp, repo) => repo.FindAllAsync(exp);

        /// <summary>
        /// Used by the Logic class to access the DB.
        /// </summary>
        protected readonly IDapperRepository<T> _genericRepo;


        public GenericCrudLogic(IDapperRepository<T> genericRepo, AbstractValidator<T> fluentValidator = null)
        {
            FluentValidator = fluentValidator;
            _genericRepo = genericRepo;
            SetupDefaultValidators();
        }

        private void SetupDefaultValidators()
        {
            #region Create
            
            var create = Validators[nameof(Create)] = new Validator<T>();
            
            create.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) && 
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw:true) &&
                IsNotNull(param.Entity, ref errors);
            
            create.EntityValidation = FluentValidator == null ? 
                Validator<T>.DataAnnotationEntityValidation :
                (CrudParam<T> x, ref List<ValidationError> y) => Validator<T>.FluentEntityValidation(x, ref y, FluentValidator);
            
            create.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region Read
            
            var read = Validators[nameof(GetById)] = new Validator<T>();

            read.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw:true) &&
                IsNotNullEmpty(param.EntityIds, ref errors);
            
            read.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            read.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion
            
            #region Update
            
            var update = Validators[nameof(Update)] = new Validator<T>();
            
            update.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw:true) &&
                IsNotNull(param.Entity, ref errors) &&
                IsNotNull(param.Entity.Id, ref errors);
            
            update.EntityValidation = FluentValidator == null ? 
                Validator<T>.DataAnnotationEntityValidation :
                (CrudParam<T> x, ref List<ValidationError> y) => Validator<T>.FluentEntityValidation(x, ref y, FluentValidator);
            
            update.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region PartialUpdate
            
            var partial = Validators[nameof(PartialUpdate)] = new Validator<T>();
            
            partial.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw:true) &&
                IsNotNullEmpty(param.EntityIds, ref errors) &&
                IsNotNull(param.Patch, ref errors);

            partial.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            partial.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region Delete
            
            var delete = Validators[nameof(Delete)] = new Validator<T>();
            delete.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw:true) &&
                IsNotNullEmpty(param.EntityIds, ref errors);

            delete.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;

            delete.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;

            #endregion
            
            #region GetByOwnerId
            
            var byOwnerId = Validators[nameof(GetByOwnerId)] = new Validator<T>();

            byOwnerId.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                IsNotNull(param.Requester, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw:true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw:true);
            
            byOwnerId.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            byOwnerId.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion
        }
        
        // Set @throw true if API is responsible of error
        protected bool IsNotNull<J>(J item, ref List<ValidationError> errors, bool @throw = false)
        {
            if (item != null)
                return true;
            
            var error = new ValidationError(typeof(J).Name, "Argument can't be null.");
            errors.Add(error);
            
            if (@throw)
                throw new ArgumentNullException(typeof(J).Name);
            
            return false;
        }
        protected bool IsNotNullEmpty<J>(IEnumerable<J> items, ref List<ValidationError> errors, bool @throw = false)
        {
            if (items != null && items.Count() > 0)
                return true;
            
            var error = new ValidationError(typeof(J).Name, "Argument can't be null or empty.");
            errors.Add(error);
            
            if (@throw)
                throw new ArgumentException("Argument can't be null or empty.", typeof(J).Name);

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
        
        
        
        public virtual async Task<ApiResult<int?>> Create(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Create)].Validate(param, ref errors))
                return new ApiResult<int?>() { Result = null, Successful = false, Errors = errors };

            await _genericRepo.InsertAsync(param.Entity);

            return new ApiResult<int?>(param.Entity.Id);
        }

        public virtual async Task<ApiResult<IEnumerable<T>>> GetById(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(GetById)].Validate(param, ref errors))
                return new ApiResult<IEnumerable<T>>() { Result = null, Successful = false, Errors = errors };

            var entities = await DbReadChildSelector(x => param.EntityIds.Contains(x.Id), _genericRepo);

            return new ApiResult<IEnumerable<T>>(entities);
        }

        // ** Are the Update/PartialUpdate Actions flexible and secure enough?
        
        public virtual  async Task<ApiResult<bool?>> Update(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Update)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };
            
            var success = await _genericRepo.UpdateAsync(param.Entity);
            
            return new ApiResult<bool?>(success);
        }

        public virtual async Task<ApiResult<bool?>> PartialUpdate(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(PartialUpdate)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };
            
            var original = await _genericRepo.FindByIdAsync(param.EntityIds.First());
            param.Patch.ApplyTo(original);
            param.Entity = original;
            return await Update(param);
        }
        public virtual  async Task<ApiResult<bool?>> Delete(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Delete)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };

            var success = await _genericRepo.DeleteAsync(x => param.EntityIds.Contains(x.Id));

            return new ApiResult<bool?>(success);        
        }
        
        // ** This is far from being elegant. How else can we introduce IOwnedEntity constraint without making the whole GenericCrud constrained to it?
        /// <summary>
        /// You currently need to inject dependencies directly to method due to design conflicts. A better structure will be introduced later.
        /// </summary>
        public static async Task<ApiResult<IEnumerable<J>>> GetByOwnerId<J>(CrudParam<J> param, IGenericCrudLogic<J> logic, IDapperRepository<J> repo) where J : class, IIdEntity, IOwnedEntity
        {
            var errors = new List<ValidationError>();
            if (!logic.Validators[nameof(GetByOwnerId)].Validate(param, ref errors))
                return new ApiResult<IEnumerable<J>>() { Result = null, Successful = false, Errors = errors };

            var requesterId = int.Parse(param.Requester.Identity.Name);
            
            var entities = await logic.DbReadChildSelector(x => x.OwnerId == requesterId, repo);

            return new ApiResult<IEnumerable<J>>(entities);
        }
    }
}