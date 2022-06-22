using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace GenericCRUD
{
    /// <summary>
    /// An API result wrapper for delivering metadata along the result object.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class ApiResult<T>
    {
        public T Result { get; set; }
        public bool Successful { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public List<string> Messages { get; set; } = new();

        public ApiResult() { }
        public ApiResult(T result) 
        { 
            Result = result; 
            Successful = result != null;
        }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}