using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MicroOrm.Dapper.Repositories.Attributes;
using MicroOrm.Dapper.Repositories.Attributes.Joins;

namespace GenericCRUD.Sample.Models
{
    [Table("todolists")]
    public class ToDoList : IIdEntity
    {
        [Identity, Key]
        public int Id { get; set; }
        
        [IgnoreUpdate]
        public int OwnerId { get; set; }

        public string Name { get; set; }
        
        [LeftJoin("todoitems", "Id", "ListId")]
        public List<ToDoItem> Items { get; set; }
    }
}