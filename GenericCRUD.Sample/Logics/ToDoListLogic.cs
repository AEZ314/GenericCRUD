using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using GenericCRUD.Sample.Models;
using MicroOrm.Dapper.Repositories;

namespace GenericCRUD.Sample.Logics
{
    public class ToDoListLogic : GenericCrudLogic<ToDoList>
    {
        public ToDoListLogic(IDapperRepository<ToDoList> genericRepo) : base(genericRepo)
        {
            DbReadSelector = (param, repo) => repo.FindAllAsync<ToDoItem>(x => param.EntityIds.Contains(x.Id), x => x.Items);
            
            Validators[nameof(GetById)].AuthorityValidation = (CrudParam<ToDoList> param, ref List<ValidationError> errors) => Validator<ToDoList>.RDValidation(param, ref errors, _genericRepo);
            Validators[nameof(Update)].AuthorityValidation = (CrudParam<ToDoList> param, ref List<ValidationError> errors) => Validator<ToDoList>.UValidation(param, ref errors, _genericRepo);
            Validators[nameof(Delete)].AuthorityValidation = (CrudParam<ToDoList> param, ref List<ValidationError> errors) => Validator<ToDoList>.RDValidation(param, ref errors, _genericRepo);
        }

        public override Task<ApiResult<int?>> Create(CrudParam<ToDoList> param)
        {
            param.Entity.OwnerId = int.Parse(param.Requester.Identity.Name);
            return base.Create(param);
        }
    }
}