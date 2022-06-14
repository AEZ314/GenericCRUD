using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericCRUD.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenericCRUD.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ToDoListController : GenericCrudBaseController<ToDoList>
    {
        private readonly IGenericCrudLogic<ToDoList> _logic;
    
        public ToDoListController(IGenericCrudLogic<ToDoList> logic) : base(logic)
        {
            _logic = logic;
            logic.DbReadSelector = (param, repo) => repo.FindAllAsync<ToDoItem>(x => param.EntityIds.Contains(x.Id), x => x.Items);
        }
    }
}