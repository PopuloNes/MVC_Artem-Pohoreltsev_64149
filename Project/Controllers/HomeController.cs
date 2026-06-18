using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceReader.Models;

namespace RaceReader.Controllers;

public class HomeController : Controller
{
    private readonly RaceReader.Data.ApplicationDbContext _context;

    public HomeController(RaceReader.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int? categoryId, string? searchQuery)
    {
        var books = _context.Books.Include(b => b.Ratings).AsQueryable();
        if (categoryId.HasValue)
        {
            books = books.Where(b => b.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            books = books.Where(b => b.Title.ToLower().Contains(searchQuery.ToLower()) || 
                                     b.Author.ToLower().Contains(searchQuery.ToLower()));
        }

        var vm = new HomeViewModel
        {
            Books = books.ToList(),
            Categories = _context.Categories.ToList()
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Pricing(bool outOfTokens = false)
    {
        ViewBag.OutOfTokens = outOfTokens;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
