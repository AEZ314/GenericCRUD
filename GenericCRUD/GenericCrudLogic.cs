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
            var create = Validators[nameof(Create)] = new Validator<T>();
            create.ParameterValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            //create.EntityValidation = Validator<T>.DataAnnotationEntityValidation;
            create.EntityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            create.AuthorityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            
            var read = Validators[nameof(GetById)] = new Validator<T>();
            read.ParameterValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            read.EntityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            read.AuthorityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            
            var update = Validators[nameof(Update)] = new Validator<T>();
            update.ParameterValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            update.EntityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            update.AuthorityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            
            var delete = Validators[nameof(Delete)] = new Validator<T>();
            delete.ParameterValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            delete.EntityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
            delete.AuthorityValidation = (CrudParam<T> param, ref IEnumerable<Exception> errors) =>
            {
                return true;
            };
        }
        
        
        protected bool NullCheck<J>(J item, ref List<Exception> errors)
        {
            if (item != null)
                return true;
            
            errors.Add(new ArgumentNullException(item.GetType().Name));
            return false;
        }
        protected bool EnumerableLengthCheck<J>(IEnumerable<J> items, ref List<Exception> errors)
        {
            if (items != null && items.Count() > 0)
                return true;

            errors.Add(new ArgumentException("Argument can't be null or empty.", items.GetType().Name));

            return false;
        }
        protected bool IntCheck(ref List<Exception> errors, int integer, int inclusiveMin = int.MinValue, int inclusiveMax = int.MaxValue)
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

        public ApiResult<bool?> PartialUpdate(CrudParam<T> param, JsonPatchDocument<T> patchDoc)
        {
            throw new System.NotImplementedException();
        }

        public ApiResult<bool?> Delete(CrudParam<T> param)
        {
            throw new System.NotImplementedException();
        }
    }
}