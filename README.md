# GenericCRUD
Don't repeat yourself! GenericCrud is a highly flexible library that provides your API with a skeleton for CRUD controllers without using EF. You can configure model validation, user permission validation etc.

### NuGet
https://www.nuget.org/packages/GenericCRUD/

# Documentation
### Overall view
GenericCrud provides a default crud system into which you can inject your custom CRUD Actions, logic, validators etc.

  GenericCrudControllerBase&#60;T&#62; ← GenericCrudLogic&#60;T&#62; ← DapperRepository&#60;T&#62; <br />
$~~~~~~~~~~~~~~~~~~~~~$↓$~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~$↓$~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~$↓<br />
CustomCrudController&#60;T&#62; ← CustomLogic&#60;T&#62; ← CustomRepository&#60;T&#62; <br />
$~~~~~~~~~~~~~~~~$↓<br />
 Access from API Clients
  
### Setup
    public void ConfigureServices(IServiceCollection services)
    {
      // Setup MicroOrm.Dapper.Repositories
      MicroOrmConfig.SqlProvider = SqlProvider.MySQL;
      services.AddSingleton(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));
      services.AddTransient<IDbConnection>(sp => new MySqlConnection(Configuration.GetConnectionString("main")));

      // Add DapperRepository DI for specific entity
      services.AddTransient<IDapperRepository<ToDoItem>>(sp => new DapperRepository<ToDoItem>(sp.GetService<IDbConnection>()));
      // Add default GenericCrudLogic DI for specific entity
      services.AddTransient<IGenericCrudLogic<ToDoItem>>(sp => new GenericCrudLogic<ToDoItem>(sp.GetService<IDapperRepository<ToDoItem>>()));

      services.AddTransient<IDapperRepository<ToDoList>>(sp => new DapperRepository<ToDoList>(sp.GetService<IDbConnection>()));
      // Add custom class deriving from GenericCrudLogic
      services.AddTransient<ToDoListLogic>(sp => new ToDoListLogic(sp.GetService<IDapperRepository<ToDoList>>()));
            
      ...
    }
    
### Controller with no additional/overriding actions
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ToDoItemController : GenericCrudBaseController<ToDoItem>
    {
        private readonly IGenericCrudLogic<ToDoItem> _logic;
    
        public ToDoItemController(IGenericCrudLogic<ToDoItem> logic) : base(logic)
        {
            _logic = logic;
        }
    }
    
### Controller with additional/overriding actions
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ToDoListController : GenericCrudBaseController<ToDoList>
    {
        private readonly ToDoListLogic _logic;
    
        public ToDoListController(ToDoListLogic logic) : base(logic)
        {
            _logic = logic;
        }
        
        [Route("[Action]")]
        [HttpGet]
        public Task<ApiResult<IEnumerable<ToDoList>>> GetByOwnerId()
        {
            return _logic.GetByOwnerId(new CrudParam<ToDoList>()
            {
                Requester = HttpContext.User
            });
        }
    }
    
### Logic Class with additional/overriding logic
Note: If you don't add/override logic you just use the GenericCrudLogic which is the default implementation of IGenericCrudLogic, whereas you create a Controller class regardless of it being custom or default.
    
    public class ToDoListLogic : GenericCrudLogic<ToDoList>
    {
        public ToDoListLogic(IDapperRepository<ToDoList> genericRepo) : base(genericRepo)
        {
            DbReadChildSelector = (exp, repo) => repo.FindAllAsync<ToDoItem>(exp, x => x.Items);
            
            Validators[nameof(GetById)].AuthorityValidation = (CrudParam<ToDoList> param, ref List<ValidationError> errors) => Validator<ToDoList>.RDValidation(param, ref errors, _genericRepo);
            Validators[nameof(Update)].AuthorityValidation = (CrudParam<ToDoList> param, ref List<ValidationError> errors) => Validator<ToDoList>.UValidation(param, ref errors, _genericRepo);
            Validators[nameof(Delete)].AuthorityValidation = (CrudParam<ToDoList> param, ref List<ValidationError> errors) => Validator<ToDoList>.RDValidation(param, ref errors, _genericRepo);
        }

        public override Task<ApiResult<int?>> Create(CrudParam<ToDoList> param)
        {
            param.Entity.OwnerId = int.Parse(param.Requester.Identity.Name);
            return base.Create(param);
        }
        
        public Task<ApiResult<IEnumerable<ToDoList>>> GetByOwnerId(CrudParam<ToDoList> param)
        {
            return GenericCrudLogic<ToDoList>.GetByOwnerId(param, this, _genericRepo);
        }
    }
