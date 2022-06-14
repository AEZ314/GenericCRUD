﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace GenericCRUD
{
    public class ApiResult<T>
    {
        public T Result { get; set; }
        public bool Successful { get; set; }
        public List<Exception> Errors { get; set; } = new();
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