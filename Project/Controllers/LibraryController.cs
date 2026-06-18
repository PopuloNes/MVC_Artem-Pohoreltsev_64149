using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceReader.Data;
using RaceReader.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RaceReader.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);

            var libraryItems = await _context.UserLibraries
                .Include(l => l.Book)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var readingSessions = await _context.ReadingSessions
                .Include(s => s.Book)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var vm = new LibraryViewModel
            {
                TokenBalance = user.TokenBalance,
                LibraryItems = libraryItems,
                Transactions = transactions,
                ReadingSessions = readingSessions
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int bookId, string status)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var item = await _context.UserLibraries.FirstOrDefaultAsync(l => l.BookId == bookId && l.UserId == userId);

            if (item != null)
            {
                item.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class LibraryViewModel
    {
        public int TokenBalance { get; set; }
        public List<UserLibrary> LibraryItems { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<ReadingSession> ReadingSessions { get; set; }
    }
}
