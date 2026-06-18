using System.Collections.Generic;

namespace RaceReader.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
