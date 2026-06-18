using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceReader.Data;
using RaceReader.Models;
using System.Threading.Tasks;

namespace RaceReader.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .Include(b => b.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int bookId, string text)
        {
            if (!User.Identity.IsAuthenticated) return Challenge();

            if (!string.IsNullOrWhiteSpace(text))
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var comment = new Comment
                {
                    BookId = bookId,
                    Text = text,
                    UserId = userId
                };
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = bookId });
        }
    }
}
