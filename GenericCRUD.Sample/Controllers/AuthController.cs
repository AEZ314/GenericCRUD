using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using GenericCRUD.Sample.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenericCRUD.Sample.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        [HttpGet]
        [Authorize]
        [Route("whoami")]
        public async Task<string> WhoAmI()
        {
            var res = "";
            HttpContext.User.Claims.AsList().ForEach(x => res += $"{x.Type}: {x.Value}\n");
            return res;
        }
        
        [HttpGet]
        [Route("login")]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginCallback([FromForm]string email, [FromForm] string password, [FromForm] string name)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
            };
            var claimsIdentity = new ClaimsIdentity(claims, "cookie");
            var principal = new ClaimsPrincipal(claimsIdentity);
            
            await HttpContext.SignInAsync("cookie", principal);
            
            return Ok($"logged in {name}!");
        }
        
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("cookie");
            
            return Ok("You're logged out");
        }
    }
}