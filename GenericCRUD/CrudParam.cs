using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Security.Claims;

namespace GenericCRUD
{
    public class CrudParam<T>
    {
        public ClaimsPrincipal Requester { get; set; }
        public T Entity { get; set; }
        public IEnumerable<int> EntityIds { get; set; }
    }
}