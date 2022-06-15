using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericCRUD.Sample.Logics;
using GenericCRUD.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenericCRUD.Sample.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ToDoListController : GenericCrudBaseController<ToDoList>
    {
        private readonly IGenericCrudLogic<ToDoList> _logic;
    
        public ToDoListController(ToDoListLogic logic) : base(logic)
        {
            _logic = logic;
        }
    }
}