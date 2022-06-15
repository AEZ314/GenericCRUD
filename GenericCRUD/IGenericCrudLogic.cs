using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MicroOrm.Dapper.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace GenericCRUD
{
    public interface IGenericCrudLogic<T> where T : class, IIdEntity
    {
        Dictionary<string, Validator<T>> Validators { get; set; }
        Func<Expression<Func<T, bool>>, IDapperRepository<T>, Task<IEnumerable<T>>> DbReadChildSelector { get; set; }
        
        Task<ApiResult<int?>> Create(CrudParam<T> param);
        Task<ApiResult<bool?>> Delete(CrudParam<T> param);
        Task<ApiResult<IEnumerable<T>>> GetById(CrudParam<T> param);
        Task<ApiResult<bool?>> PartialUpdate(CrudParam<T> param);
        Task<ApiResult<bool?>> Update(CrudParam<T> param);
    }
}