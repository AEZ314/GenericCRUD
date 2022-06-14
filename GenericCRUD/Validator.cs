using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;

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
    }
}