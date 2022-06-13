using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace GenericCRUD
{
    public interface IGenericCrudLogic<T> where T : class
    {
        Dictionary<string, Validator<T>> Validators { get; set; }

        ApiResult<int?> Create(CrudParam<T> param);
        ApiResult<int?> Create(CrudParam<T> param, T canvas);
        ApiResult<bool?> Delete(CrudParam<T> param);
        ApiResult<IEnumerable<T>> GetById(CrudParam<T> param);
        ApiResult<bool?> PartialUpdate(CrudParam<T> param, JsonPatchDocument<T> patchDoc);
        ApiResult<bool?> Update(CrudParam<T> param);
    }
}