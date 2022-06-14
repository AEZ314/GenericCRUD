using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.Attributes;

namespace GenericCRUD.Sample.Models
{
    [Table("todoitems")]
    public class ToDoItem : IIdEntity
    {
        [Identity, Key]
        public int Id { get; set; }
        
        [IgnoreUpdate]
        public int ListId { get; set; }

        public string Text { get; set; }
    }
}