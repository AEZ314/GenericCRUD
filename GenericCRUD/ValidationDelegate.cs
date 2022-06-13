using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GenericCRUD
{
    public delegate bool ValidationDelegate<T>(CrudParam<T> param, ref IEnumerable<string> errors);
}