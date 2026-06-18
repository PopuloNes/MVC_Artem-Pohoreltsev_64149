using System;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class UserLibrary
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
