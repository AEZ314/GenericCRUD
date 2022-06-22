using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;

namespace GenericCRUD
{
    /// <summary>
    /// A generic, hardcoded manifesto for a CRUD API action. Enables the code to process a variety of CRUD actions.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class CrudParam<T> where T : class, IIdEntity
    {
        public ClaimsPrincipal Requester { get; set; }
        public T Entity { get; set; }
        public JsonPatchDocument<T> Patch { get; set; }
        public List<int> EntityIds { get; set; }
    }
}