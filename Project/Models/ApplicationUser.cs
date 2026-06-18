using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace RaceReader.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int TokenBalance { get; set; } = 60; 
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<ReadingSession> ReadingSessions { get; set; }
        public virtual ICollection<UserLibrary> UserLibraries { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
    }
}
