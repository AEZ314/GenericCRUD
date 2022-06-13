using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GenericCRUD
{
    public class Validator<T>
    {
        public ValidationDelegate<T> ParameterValidation { get; set; } = (CrudParam<T> param, ref IEnumerable<ValidationResult> errors) => true;
        public ValidationDelegate<T> EntityValidation { get; set; } = (CrudParam<T> param, ref IEnumerable<ValidationResult> errors) => true;
        public ValidationDelegate<T> AuthorityValidation { get; set; } = (CrudParam<T> param, ref IEnumerable<ValidationResult> errors) => true;

        public virtual bool Validate(CrudParam<T> param, ref IEnumerable<ValidationResult> errors)
        {
            return ParameterValidation(param, ref errors) && 
                   EntityValidation(param, ref errors) && 
                   AuthorityValidation(param, ref errors);
        }
    }
}