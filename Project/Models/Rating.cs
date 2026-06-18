using System;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class Rating
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
        [Range(1, 5)]
        public int Score { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
