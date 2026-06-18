using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } 
        public virtual ICollection<Book> Books { get; set; }
    }
}
