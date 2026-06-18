using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Linq;
using RaceReader.Models;

namespace RaceReader.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@racereader.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    TokenBalance = 1000 // Give admin some tokens for testing
                };

                var createPowerUser = await userManager.CreateAsync(user, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Create test user
            var testEmail = "test@racereader.com";
            var testUser = await userManager.FindByEmailAsync(testEmail);
            if (testUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    EmailConfirmed = true,
                    TokenBalance = 0 // 0 tokens to test purchasing
                };

                var createTestUser = await userManager.CreateAsync(user, "Test123!");
                if (createTestUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }

            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            if (!dbContext.Categories.Any())
            {
                dbContext.Categories.AddRange(
                    new Category { Name = "Autobiography" },
                    new Category { Name = "Magazine" },
                    new Category { Name = "Technical / Engineering" },
                    new Category { Name = "History" }
                );
                await dbContext.SaveChangesAsync();
            }

            // Seed comments and ratings
            var testUserFromDb = await userManager.FindByEmailAsync("test@racereader.com");
            var adminUserFromDb = await userManager.FindByEmailAsync("admin@racereader.com");

            if (testUserFromDb != null && adminUserFromDb != null)
            {
                var books = dbContext.Books.ToList();
                foreach (var book in books)
                {
                    // Check if comments exist
                    if (!dbContext.Comments.Any(c => c.BookId == book.Id))
                    {
                        dbContext.Comments.AddRange(
                            new Comment { BookId = book.Id, UserId = testUserFromDb.Id, Text = "Great book, highly recommend it! An absolute must-read for any motorsport fan.", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                            new Comment { BookId = book.Id, UserId = adminUserFromDb.Id, Text = "Found it a bit technical in some chapters, but overall very interesting.", CreatedAt = DateTime.UtcNow.AddDays(-1) }
                        );
                    }

                    // Check if ratings exist
                    if (!dbContext.Ratings.Any(r => r.BookId == book.Id))
                    {
                        dbContext.Ratings.AddRange(
                            new Rating { BookId = book.Id, UserId = testUserFromDb.Id, Score = 5, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                            new Rating { BookId = book.Id, UserId = adminUserFromDb.Id, Score = 4, CreatedAt = DateTime.UtcNow.AddDays(-1) }
                        );
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
