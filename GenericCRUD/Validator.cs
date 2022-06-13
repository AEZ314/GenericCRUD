using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dapper;

namespace GenericCRUD
{
    public class Validator<T> where T : class, IIdEntity
    {
        public ValidationDelegate<T> ParameterValidation { get; set; } = (CrudParam<T> param, ref List<Exception> errors) => true;
        public ValidationDelegate<T> EntityValidation { get; set; } = (CrudParam<T> param, ref List<Exception> errors) => true;
        public ValidationDelegate<T> AuthorityValidation { get; set; } = (CrudParam<T> param, ref List<Exception> errors) => true;

        public virtual bool Validate(CrudParam<T> param, ref List<Exception> errors)
        {
            return ParameterValidation(param, ref errors) && 
                   EntityValidation(param, ref errors) && 
                   AuthorityValidation(param, ref errors);
        }
        
        
        public static bool DataAnnotationEntityValidation(CrudParam<T> param, ref List<Exception> errors)
        {
            var annotationsErrors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(param.Entity, new ValidationContext(param.Entity), annotationsErrors, true);

            foreach (var error in annotationsErrors)
            {
                errors.Add(new ArgumentException(error.ErrorMessage));
            }
            
            return isValid;
        }
    }
}