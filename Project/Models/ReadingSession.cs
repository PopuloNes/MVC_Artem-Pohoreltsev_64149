using System;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class ReadingSession
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int BookId { get; set; }
        public virtual Book Book { get; set; }
        public int LastPageRead { get; set; } = 1;
        public int TotalTokensSpent { get; set; } = 0;
        public int TotalMinutesSpent { get; set; } = 0;
        public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
    }
}
