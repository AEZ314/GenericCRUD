using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GenericCRUD.Sample.Logics;
using GenericCRUD.Sample.Models;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.Config;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MySqlConnector;

namespace GenericCRUD.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            MicroOrmConfig.SqlProvider = SqlProvider.MySQL;
            services.AddSingleton(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));
            services.AddTransient<IDbConnection>(sp => new MySqlConnection(Configuration.GetConnectionString("main")));

            
            
            services.AddTransient<IDapperRepository<ToDoItem>>(sp => new DapperRepository<ToDoItem>(sp.GetService<IDbConnection>()));
            services.AddTransient<IGenericCrudLogic<ToDoItem>>(sp => new GenericCrudLogic<ToDoItem>(sp.GetService<IDapperRepository<ToDoItem>>()));

            services.AddTransient<IDapperRepository<ToDoList>>(sp => new DapperRepository<ToDoList>(sp.GetService<IDbConnection>()));
            services.AddTransient<ToDoListLogic>(sp => new ToDoListLogic(sp.GetService<IDapperRepository<ToDoList>>()));



            services.AddAuthorization();
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = "cookie";
                opt.DefaultChallengeScheme= "cookie";
                opt.DefaultSignInScheme = "cookie";
                opt.DefaultSignOutScheme = "cookie";
            })
                .AddCookie("cookie", opt =>
                {
                    opt.Cookie.Name = "AuthCookie";
                    opt.LoginPath = "/auth/login";
                    opt.LogoutPath = "/auth/logout";
                });
            
            
            
            services.AddControllers();
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "GenericCRUD.Sample", Version = "v1"});
                // Custom naming to later enable generics
                c.CustomSchemaIds(t => 
                {
                    string iter(Type t)
                    {
                        if (!t.IsGenericType)
                            return t.Name;
                        
                        var generics = t.GetGenericArguments();
                        
                        var res = t.Name.Substring(0, t.Name.IndexOf("`")) + "<";

                        foreach (var generic in generics)
                        {
                            res += iter(generic);

                            if (generic != generics.Last())
                                res += ",";
                        }
                        
                        return res + ">";
                    }
                    
                    return iter(t);
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GenericCRUD.Sample v1");
                    c.InjectStylesheet("/SwaggerDark.css");
                });
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}