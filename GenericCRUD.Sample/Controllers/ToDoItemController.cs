using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary1;
using GenericCRUD.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenericCRUD.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ToDoItemController : GenericCrudBaseController<ToDoItem>
    {
        private readonly IGenericCrudLogic<ToDoItem> _logic;
    
        public ToDoItemController(IGenericCrudLogic<ToDoItem> logic) : base(logic)
        {
            _logic = logic;
        }
    }
}