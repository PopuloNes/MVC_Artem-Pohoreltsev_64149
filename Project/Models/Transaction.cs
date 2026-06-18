using System;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string StripeSessionId { get; set; }
        public decimal Amount { get; set; } 
        public int TokensAdded { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } 
    }
}
