using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceReader.Data;
using RaceReader.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RaceReader.Controllers
{
    [Authorize]
    public class ReaderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReaderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Read(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user.TokenBalance <= 0)
            {
                // Redirect to pricing if out of tokens
                return RedirectToAction("Pricing", "Home", new { outOfTokens = true });
            }

            var session = await _context.ReadingSessions
                .FirstOrDefaultAsync(s => s.BookId == id && s.UserId == userId);

            if (session == null)
            {
                session = new ReadingSession
                {
                    BookId = id,
                    UserId = userId,
                    LastPageRead = 1
                };
                _context.ReadingSessions.Add(session);
                await _context.SaveChangesAsync();
            }
            
            // Add to library if not there
            var libraryEntry = await _context.UserLibraries.FirstOrDefaultAsync(l => l.BookId == id && l.UserId == userId);
            if(libraryEntry == null)
            {
                _context.UserLibraries.Add(new UserLibrary { BookId = id, UserId = userId, Status = "Reading" });
                await _context.SaveChangesAsync();
            }
            else if(libraryEntry.Status == "Planned")
            {
                libraryEntry.Status = "Reading";
                await _context.SaveChangesAsync();
            }

            ViewBag.TokenBalance = user.TokenBalance;
            ViewBag.SessionId = session.Id;
            ViewBag.StartPage = session.LastPageRead;

            return View(book);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // allowing simple JS fetch
        public async Task<IActionResult> DeductTokenAndSaveProgress([FromBody] ProgressUpdateModel data)
        {
            if (data == null) return BadRequest();

            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            var session = await _context.ReadingSessions.FindAsync(data.SessionId);

            if (user == null || session == null || session.UserId != userId)
                return Unauthorized();

            if (user.TokenBalance <= 0)
                return Json(new { success = false, tokens = 0, message = "Out of tokens" });

            // Deduct 1 token per minute ping
            user.TokenBalance -= 1;
            
            // Update session
            session.LastPageRead = data.Page;
            session.TotalTokensSpent += 1;
            session.TotalMinutesSpent += 1;
            session.LastReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, tokens = user.TokenBalance });
        }
        
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveProgress([FromBody] ProgressUpdateModel data)
        {
            // Just saves page, without deducting token (for when user turns page)
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var session = await _context.ReadingSessions.FindAsync(data.SessionId);

            if (session != null && session.UserId == userId)
            {
                session.LastPageRead = data.Page;
                await _context.SaveChangesAsync();
            }
            
            return Ok();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RateBook([FromBody] RateBookModel data)
        {
            if (data == null || data.Score < 1 || data.Score > 5) return BadRequest();

            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var rating = await _context.Ratings.FirstOrDefaultAsync(r => r.BookId == data.BookId && r.UserId == userId);
            
            if (rating == null)
            {
                rating = new Rating { BookId = data.BookId, UserId = userId, Score = data.Score };
                _context.Ratings.Add(rating);
            }
            else
            {
                rating.Score = data.Score;
                rating.CreatedAt = DateTime.UtcNow; // update timestamp
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class RateBookModel
    {
        public int BookId { get; set; }
        public int Score { get; set; }
    }

    public class ProgressUpdateModel
    {
        public int SessionId { get; set; }
        public int Page { get; set; }
    }
}
