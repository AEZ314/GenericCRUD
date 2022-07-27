using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using MicroOrm.Dapper.Repositories;

namespace GenericCRUD
{
    public class GenericCrudLogicConfigurator
    {
        public void SetupDefaultValidators<T>(IGenericCrudLogic<T> logic, AbstractValidator<T> fluentValidator = null) where T : class, IIdEntity
        {
            bool requesterCheck(CrudParam<T> param, ref List<ValidationError> errors) => 
                IsNotNull(param.Requester, ref errors, @throw: true) &&
                IsNotNull(param.Requester.Identity, ref errors, @throw: true) &&
                IsNotNull(param.Requester.Identity.Name, ref errors, @throw: true);

            #region Create
            
            var create = logic.Validators[nameof(logic.Create)] = new GenericCrudValidator<T>();
            
            create.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) && 
                requesterCheck(param, ref errors) &&
                IsNotNull(param.Entity, ref errors);
            
            create.EntityValidation = fluentValidator == null ? 
                DataAnnotationEntityValidation :
                (CrudParam<T> x, ref List<ValidationError> y) => FluentEntityValidation(x, ref y, fluentValidator);
            
            create.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region Read
            
            var read = logic.Validators[nameof(logic.GetById)] = new GenericCrudValidator<T>();

            read.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                requesterCheck(param, ref errors) &&
                IsNotNullEmpty(param.EntityIds, ref errors);
            
            read.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            read.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion
            
            #region Update
            
            var update = logic.Validators[nameof(logic.Update)] = new GenericCrudValidator<T>();
            
            update.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                requesterCheck(param, ref errors) &&
                IsNotNull(param.Entity, ref errors) &&
                IsNotNull(param.Entity.Id, ref errors);
            
            update.EntityValidation = fluentValidator == null ? 
                DataAnnotationEntityValidation :
                (CrudParam<T> x, ref List<ValidationError> y) => FluentEntityValidation(x, ref y, fluentValidator);
            
            update.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region PartialUpdate
            
            var partial = logic.Validators[nameof(logic.PartialUpdate)] = new GenericCrudValidator<T>();
            
            partial.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                requesterCheck(param, ref errors) &&
                IsNotNullEmpty(param.EntityIds, ref errors) &&
                IsNotNull(param.Patch, ref errors);

            partial.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            partial.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion

            #region Delete
            
            var delete = logic.Validators[nameof(logic.Delete)] = new GenericCrudValidator<T>();
            delete.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw:true) &&
                requesterCheck(param, ref errors) &&
                IsNotNullEmpty(param.EntityIds, ref errors);

            delete.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            delete.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;

            #endregion
            
            #region GetByOwnerId
            
            var byOwnerId = logic.Validators[nameof(GenericCrudLogic<T>.GetByOwnerId)] = new GenericCrudValidator<T>();

            byOwnerId.ParameterValidation = (CrudParam<T> param, ref List<ValidationError> errors) =>
                IsNotNull(param, ref errors, @throw: true) &&
                requesterCheck(param, ref errors);
            
            byOwnerId.EntityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            byOwnerId.AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => true;
            
            #endregion
        }

        public void SetupDefaultAuthorityValidators<T>(IGenericCrudLogic<T> logic) where T : class, IIdEntity, IOwnedEntity
        {
            logic.Validators[nameof(logic.GetById)].AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => RDValidation(param, ref errors, logic.GenericRepo);
            logic.Validators[nameof(logic.Update)].AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => UValidation(param, ref errors, logic.GenericRepo);
            logic.Validators[nameof(logic.Delete)].AuthorityValidation = (CrudParam<T> param, ref List<ValidationError> errors) => RDValidation(param, ref errors, logic.GenericRepo);
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

        protected static bool DataAnnotationEntityValidation<T>(CrudParam<T> param, ref List<ValidationError> errors) where T : class, IIdEntity
        {
            var annotationsErrors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(param.Entity, new ValidationContext(param.Entity), annotationsErrors, true);

            foreach (var error in annotationsErrors)
            {
                errors.Add(new ValidationError(error.MemberNames.First(), error.ErrorMessage)); // ** Adding only the First() is questionable.
            }
            
            return isValid;
        }
        protected static bool FluentEntityValidation<T>(CrudParam<T> param, ref List<ValidationError> errors, AbstractValidator<T> validator) where T : class, IIdEntity
        {
            var validationResult = validator.Validate(param.Entity);

            foreach (var error in validationResult.Errors)
            {
                errors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
            }
            
            return validationResult.IsValid;
        }

        /// <summary>
        /// Helper method checking whether all of the param.EntityIds are owned by param.Requester. Intended to be used for the validations
        /// before Read and Delete Actions.
        /// </summary>
        protected static bool RDValidation<J>(CrudParam<J> param, ref List<ValidationError> errors, IDapperRepository<J> repo) where J : class, IIdEntity, IOwnedEntity
        {
            if(!int.TryParse(param.Requester.Identity.Name, out var requesterId))
            {
                errors.Add(new ValidationError(nameof(param.Requester.Identity.Name), "Name must be an integer."));
                return false;
            }
            if (repo.Count(x => param.EntityIds.Contains(x.Id) && x.OwnerId == requesterId) != param.EntityIds.Count())
            {
                errors.Add(new ValidationError(nameof(param.EntityIds), "You can only retrieve entities owned by you."));
                return false;
            }

            return true;
        }
        /// <summary>
        /// Helper method checking whether param.Entity.Id is owned by param.Requester. Intended to be used for the validations before
        /// Update and Partial Update Actions. (Partial Update already uses Update's validation method under the hood by default)
        /// </summary>
        protected static bool UValidation<J>(CrudParam<J> param, ref List<ValidationError> errors, IDapperRepository<J> repo) where J : class, IIdEntity, IOwnedEntity
        {
            if(!int.TryParse(param.Requester.Identity.Name, out var requesterId))
            {
                errors.Add(new ValidationError(nameof(param.Requester.Identity.Name), "Name must be an integer."));
                return false;
            }
            if (repo.FindById(param.Entity.Id).OwnerId != requesterId)
            {
                errors.Add(new ValidationError(nameof(param.Requester.Identity.Name), "This action can be performed only on entities owned by you."));
                return false;
            }

            return true;
        }
    }
}
