using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;

namespace GenericCRUD
{
    public class CrudParam<T> where T : class, IIdEntity
    {
        public ClaimsPrincipal Requester { get; set; }
        public T Entity { get; set; }
        public JsonPatchDocument<T> Patch { get; set; }
        public List<int> EntityIds { get; set; }
    }
}