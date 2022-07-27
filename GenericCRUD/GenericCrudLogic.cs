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
        /// Used by the Logic class to access the DB.
        /// </summary>
        public IDapperRepository<T> GenericRepo { get; set; }
        /// <summary>
        /// Maps CRUD methods' names to their respective validation methods. Default CRUD method implementations use
        /// nameof(Method) to get their validators. You can likewise override existing validators through this dictionary.
        /// </summary>
        public Dictionary<string, GenericCrudValidator<T>> Validators { get; set; } = new();

        /// <summary>
        /// Set this to configure DB Entity mappings. Eg: [Join] attributes. This is related to the inner workings of dapper-extensions lib.
        /// </summary>
        public Func<Expression<Func<T, bool>>, Task<IEnumerable<T>>> DbReadChildSelector { get; set; }


        public GenericCrudLogic(IDapperRepository<T> genericRepo)
        {
            DbReadChildSelector = DefaultDbReadSelector;
            GenericRepo = genericRepo;
        }
        
        
        public virtual async Task<ApiResult<int?>> Create(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Create)].Validate(param, ref errors))
                return new ApiResult<int?>() { Result = null, Successful = false, Errors = errors };

            await GenericRepo.InsertAsync(param.Entity);

            return new ApiResult<int?>(param.Entity.Id);
        }

        public virtual async Task<ApiResult<IEnumerable<T>>> GetById(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(GetById)].Validate(param, ref errors))
                return new ApiResult<IEnumerable<T>>() { Result = null, Successful = false, Errors = errors };

            var entities = await DbReadChildSelector(x => param.EntityIds.Contains(x.Id));

            return new ApiResult<IEnumerable<T>>(entities);
        }

        // ** Are the Update/PartialUpdate Actions flexible and secure enough?
        
        public virtual  async Task<ApiResult<bool?>> Update(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Update)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };
            
            var success = await GenericRepo.UpdateAsync(param.Entity);
            
            return new ApiResult<bool?>(success);
        }

        public virtual async Task<ApiResult<bool?>> PartialUpdate(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(PartialUpdate)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };
            
            var original = await GenericRepo.FindByIdAsync(param.EntityIds.First());
            param.Patch.ApplyTo(original);
            param.Entity = original;
            return await Update(param);
        }
        public virtual  async Task<ApiResult<bool?>> Delete(CrudParam<T> param)
        {
            var errors = new List<ValidationError>();
            if (!Validators[nameof(Delete)].Validate(param, ref errors))
                return new ApiResult<bool?>() { Result = null, Successful = false, Errors = errors };

            var success = await GenericRepo.DeleteAsync(x => param.EntityIds.Contains(x.Id));

            return new ApiResult<bool?>(success);        
        }
        
        // ** This is far from being elegant. How else can we introduce IOwnedEntity constraint without making the whole GenericCrud constrained to it?
        /// <summary>
        /// You currently need to inject dependencies directly to method due to design conflicts. A better structure will be introduced later.
        /// </summary>
        public static async Task<ApiResult<IEnumerable<J>>> GetByOwnerId<J>(CrudParam<J> param, IGenericCrudLogic<J> logic) where J : class, IIdEntity, IOwnedEntity
        {
            var errors = new List<ValidationError>();
            if (!logic.Validators[nameof(GetByOwnerId)].Validate(param, ref errors))
                return new ApiResult<IEnumerable<J>>() { Result = null, Successful = false, Errors = errors };

            var requesterId = int.Parse(param.Requester.Identity.Name);
            
            var entities = await logic.DbReadChildSelector(x => x.OwnerId == requesterId);

            return new ApiResult<IEnumerable<J>>(entities);
        }
        
        
        private Task<IEnumerable<T>> DefaultDbReadSelector(Expression<Func<T, bool>> predicate) => GenericRepo.FindAllAsync(predicate);
    }
}