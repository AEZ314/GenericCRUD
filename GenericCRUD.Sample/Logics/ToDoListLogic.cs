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
            DbReadChildSelector = exp => GenericRepo.FindAllAsync<ToDoItem>(exp, x => x.Items);
            var configurator = new GenericCrudLogicConfigurator();
            configurator.SetupDefaultValidators(this);
            configurator.SetupDefaultAuthorityValidators(this);
        }

        public override Task<ApiResult<int?>> Create(CrudParam<ToDoList> param)
        {
            param.Entity.OwnerId = int.Parse(param.Requester.Identity.Name);
            return base.Create(param);
        }
        
        public Task<ApiResult<IEnumerable<ToDoList>>> GetByOwnerId(CrudParam<ToDoList> param)
        {
            return GenericCrudLogic<ToDoList>.GetByOwnerId(param, this);
        }
    }
}