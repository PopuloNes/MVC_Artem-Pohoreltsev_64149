using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaceReader.Data;
using RaceReader.Models;
using System.Threading.Tasks;
using System.Linq;

namespace RaceReader.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var books = _context.Books.Include(b => b.Category);
            return View(await books.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,Description,CategoryId")] Book book, IFormFile coverFile, IFormFile pdfFile)
        {
            ModelState.Remove("CoverImagePath");
            ModelState.Remove("PdfFilePath");
            ModelState.Remove("Category");
            ModelState.Remove("ReadingSessions");
            ModelState.Remove("UserLibraries");
            ModelState.Remove("Comments");
            ModelState.Remove("Ratings");
            if (ModelState.IsValid)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

                if (coverFile != null && coverFile.Length > 0)
                {
                    var coverFileName = Guid.NewGuid().ToString() + Path.GetExtension(coverFile.FileName);
                    var filePath = Path.Combine(uploadsPath, coverFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverFile.CopyToAsync(stream);
                    }
                    book.CoverImagePath = "/uploads/" + coverFileName;
                }

                if (pdfFile != null && pdfFile.Length > 0)
                {
                    var pdfFileName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                    var filePath = Path.Combine(uploadsPath, pdfFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(stream);
                    }
                    book.PdfFilePath = "/uploads/" + pdfFileName;
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Description,CoverImagePath,PdfFilePath,CategoryId")] Book book)
        {
            if (id != book.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("ReadingSessions");
            ModelState.Remove("UserLibraries");
            ModelState.Remove("Comments");
            ModelState.Remove("Ratings");

            if (ModelState.IsValid)
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
