using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceReader.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        [MaxLength(200)]
        public string Author { get; set; }
        
        public string Description { get; set; }
        
        public string CoverImagePath { get; set; }
        
        [Required]
        public string PdfFilePath { get; set; }
        
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<ReadingSession> ReadingSessions { get; set; }
        public virtual ICollection<UserLibrary> UserLibraries { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
    }
}
