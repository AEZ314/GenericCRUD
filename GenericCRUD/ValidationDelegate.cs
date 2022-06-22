using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GenericCRUD
{
    /// <summary>
    /// Validates the certain aspects of an entity. Different ValidationDelegates validate different aspects, such as model's properties,
    /// model's ownership, etc...
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public delegate bool ValidationDelegate<T>(CrudParam<T> param, ref List<ValidationError> errors) where T : class, IIdEntity;
}