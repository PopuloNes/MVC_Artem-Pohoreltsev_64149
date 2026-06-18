using System;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
