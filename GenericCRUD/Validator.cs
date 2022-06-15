using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;
using FluentValidation;
using MicroOrm.Dapper.Repositories;

namespace GenericCRUD
{
    public class Validator<T> where T : class, IIdEntity
    {
        public ValidationDelegate<T> ParameterValidation { get; set; } = (CrudParam<T> param, ref List<ValidationError> errors) => true;
        public ValidationDelegate<T> EntityValidation { get; set; } = (CrudParam<T> param, ref List<ValidationError> errors) => true;
        public ValidationDelegate<T> AuthorityValidation { get; set; } = (CrudParam<T> param, ref List<ValidationError> errors) => true;

        public virtual bool Validate(CrudParam<T> param, ref List<ValidationError> errors)
        {
            return ParameterValidation(param, ref errors) && 
                   EntityValidation(param, ref errors) && 
                   AuthorityValidation(param, ref errors);
        }
        
        
        public static bool DataAnnotationEntityValidation(CrudParam<T> param, ref List<ValidationError> errors)
        {
            var annotationsErrors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(param.Entity, new ValidationContext(param.Entity), annotationsErrors, true);

            foreach (var error in annotationsErrors)
            {
                errors.Add(new ValidationError(error.MemberNames.First(), error.ErrorMessage)); // ** Adding only the First() is un-comfy.
            }
            
            return isValid;
        }
        public static bool FluentEntityValidation(CrudParam<T> param, ref List<ValidationError> errors, AbstractValidator<T> validator)
        {
            var validationResult = validator.Validate(param.Entity);

            foreach (var error in validationResult.Errors)
            {
                errors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
            }
            
            return validationResult.IsValid;
        }
        
        // Basic auth validation for Read & Delete
        public static bool RDValidation<J>(CrudParam<J> param, ref List<ValidationError> errors, IDapperRepository<J> repo) where J : class, IIdEntity, IOwnedEntity
        {
            if(!int.TryParse(param.Requester.Identity.Name, out var requesterId))
            {
                errors.Add(new ValidationError(nameof(param.Requester.Identity.Name), "Name must be an integer."));
                return false;
            }
            if (repo.FindAll(x => param.EntityIds.Contains(x.Id) && x.OwnerId == requesterId).Count() != param.EntityIds.Count())
            {
                errors.Add(new ValidationError(nameof(param.EntityIds), "You can only retrieve entities owned by you."));
                return false;
            }

            return true;
        }

        // Basic auth validation for Update & PartialUpdate (Partial Update already uses Update's validation method by default)
        public static bool UValidation<J>(CrudParam<J> param, ref List<ValidationError> errors, IDapperRepository<J> repo) where J : class, IIdEntity, IOwnedEntity
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