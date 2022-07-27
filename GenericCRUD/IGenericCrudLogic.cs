using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using MicroOrm.Dapper.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace GenericCRUD
{
    /// <summary>
    /// An interface defining primitive CRUD operations. Implementors of this interface will be employed by an API Controller class.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IGenericCrudLogic<T> where T : class, IIdEntity
    {
        IDapperRepository<T> GenericRepo { get; set; }
        Dictionary<string, GenericCrudValidator<T>> Validators { get; set; }
        Func<Expression<Func<T, bool>>, Task<IEnumerable<T>>> DbReadChildSelector { get; set; }
        
        Task<ApiResult<int?>> Create(CrudParam<T> param);
        Task<ApiResult<bool?>> Delete(CrudParam<T> param);
        Task<ApiResult<IEnumerable<T>>> GetById(CrudParam<T> param);
        Task<ApiResult<bool?>> PartialUpdate(CrudParam<T> param);
        Task<ApiResult<bool?>> Update(CrudParam<T> param);
    }
}