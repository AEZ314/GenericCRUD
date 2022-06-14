using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
 using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;


namespace GenericCRUD
{
    [ApiController]
    public class GenericCrudBaseController<T> : ControllerBase where T : class, IIdEntity
    {
        protected readonly IGenericCrudLogic<T> _logic;


        public GenericCrudBaseController(IGenericCrudLogic<T> logic)
        {
            _logic = logic;
        }


        /// <summary>
        /// Create a <see cref="T"/>.
        /// </summary>
        [HttpPost]
        public virtual async Task<ApiResult<int?>> Create([FromBody] T entity)
        {
            return await _logic.Create(
                new CrudParam<T>
                {
                    Requester = User,
                    Entity = entity,
                });
        }

        /// <summary>
        /// Get <see cref="T"/> by IDs.
        /// </summary>
        [HttpGet]
        public virtual async Task<ApiResult<IEnumerable<T>>> GetById([FromQuery] List<int> ids)
        {
            return await _logic.GetById(
                new CrudParam<T>
                {
                    Requester = User,
                    EntityIds = ids,
                });
        }

        /// <summary>
        /// Update a <see cref="T"/>.
        /// </summary>
        [HttpPut]
        public virtual async Task<ApiResult<bool?>> Update([FromBody] T entity)
        {
            return await _logic.Update(
                new CrudParam<T>
                {
                    Requester = User,
                    Entity = entity,
                });
        }

        /// <summary>
        /// Partial update a <see cref="T"/>.
        /// </summary>
        [HttpPatch]
        public virtual async Task<ApiResult<bool?>> PartialUpdate([FromQuery] int id, [FromBody] JsonPatchDocument<T> patchDoc)
        {
            return await _logic.PartialUpdate(
                new CrudParam<T>
                {
                    Requester = User,
                    EntityIds = new[] { id }.AsList(),
                    Patch = patchDoc,
                });
        }

        /// <summary>
        /// Delete a <see cref="T"/>.
        /// </summary>
        [HttpDelete]
        public virtual async Task<ApiResult<bool?>> Delete([FromQuery] int id)
        {
            return await _logic.Delete(
                new CrudParam<T>
                {
                    Requester = User,
                    EntityIds = new[] { id }.AsList(),
                });
        }
    }
}