using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
 using System;
using System.Collections.Generic;
using System.Text;
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
        public virtual ApiResult<int?> Create([FromBody] T entity)
        {
            var result = _logic.Create(
                new CrudParam<T>
                {
                    Requester = User,
                    Entity = entity,
                });
            return result;
        }

        /// <summary>
        /// Get <see cref="T"/> by IDs.
        /// </summary>
        [HttpGet]
        public virtual ApiResult<IEnumerable<T>> GetById([FromQuery] List<int> ids)
        {
            var result = _logic.GetById(
                new CrudParam<T>
                {
                    Requester = User,
                    EntityIds = ids,
                });
            return result;
        }

        /// <summary>
        /// Update a <see cref="T"/>.
        /// </summary>
        [HttpPut]
        public virtual ApiResult<bool?> Update([FromBody] T entity)
        {
            var result = _logic.Update(
                new CrudParam<T>
                {
                    Requester = User,
                    Entity = entity,
                });
            return result;
        }

        /// <summary>
        /// Partial update a <see cref="T"/>.
        /// </summary>
        [HttpPatch]
        public virtual ApiResult<bool?> PartialUpdate([FromQuery] int id, [FromBody] JsonPatchDocument<T> patchDoc)
        {
            var result = _logic.PartialUpdate(
                new CrudParam<T>
                {
                    Requester = User,
                    EntityIds = new[] { id }.AsList(),
                    Patch = patchDoc,
                });
            return result;
        }

        /// <summary>
        /// Delete a <see cref="T"/>.
        /// </summary>
        [HttpDelete]
        public virtual ApiResult<bool?> Delete([FromQuery] int id)
        {
            var result = _logic.Delete(
                new CrudParam<T>
                {
                    Requester = User,
                    EntityIds = new[] { id }.AsList(),
                });
            return result;
        }
    }
}