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
                    TokenBalance = 1000 
                };

                var createPowerUser = await userManager.CreateAsync(user, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            var testEmail = "test@racereader.com";
            var testUser = await userManager.FindByEmailAsync(testEmail);
            if (testUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    EmailConfirmed = true,
                    TokenBalance = 0 
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
                var cat1 = new Category { Name = "Autobiography" };
                var cat2 = new Category { Name = "Magazine" };
                var cat3 = new Category { Name = "Technical / Engineering" };
                var cat4 = new Category { Name = "History" };
                
                dbContext.Categories.AddRange(cat1, cat2, cat3, cat4);
                await dbContext.SaveChangesAsync();
                
                if (!dbContext.Books.Any())
                {
                    dbContext.Books.AddRange(
                        new Book { Title = "Race Car Aerodynamics", Author = "Joseph Katz", Description = "A comprehensive guide to race car aerodynamics.", CategoryId = cat3.Id, CoverImagePath = "/uploads/283df83c-1cd6-45b2-9b68-489c585d4692.png", PdfFilePath = "/uploads/9e037279-bb44-47b1-8623-aeca4b076f83.pdf" },
                        new Book { Title = "To Hell And Back Niki Lauda", Author = "Damon Hill", Description = "HIGH ABOVE THE phalanx of fans clad in their red caps and \r\nshirts was a banner with a haunting picture in stark mono-\r\nchrome of a pair of eyes, scarred by heat and flame, staring \r\ndown at the bare racetrack below. After the picture was a sim-\r\nple yet heartfelt message: 'Ciao, Niki.'", CategoryId = cat1.Id, CoverImagePath = "/uploads/8afa4bfa-b45c-435c-8fe6-4eefc8e1e77d.png", PdfFilePath = "/uploads/78342d90-2708-49e5-adf5-116bff5d5607.pdf" },
                        new Book { Title = "A Guide to Motorsport Circuits of the World", Author = "Eliot Reed", Description = "Plan your ultimate motorsport adventure with this essential trackside guidebook. Compiled with invaluable inside knowledge from a vast network of professional riders, drivers, motoring journalists, and track personnel, this guide is designed to get you closer to the action and help you navigate the fast-paced world of racing.\r\nInside, you will find vital logistical information, schedules, and meticulously verified track maps to guide your journey. The practical data is brought to life by incredible, dynamic motorsport photography by Richard Dole, making it as visually captivating as it is useful.", CategoryId = cat4.Id, CoverImagePath = "/uploads/151a8c39-df7a-4a17-8feb-4b06ac940b72.png", PdfFilePath = "/uploads/c6c90745-1a24-43f0-808b-3176197b279d.pdf" },
                        new Book { Title = "Jenson_Button Life To The Limit", Author = "James Mckenna", Description = "For the old boy. Simply put I couldn't have done any of it without you. Not just \r\nbecause you're my dad, who I love dearly, but also because you were my best\r\nfriend, my confidant and my inspiration then, now and for ever. Together every \r\nstep of the way, we made our dream a reality. I love you and I miss you.", CategoryId = cat1.Id, CoverImagePath = "/uploads/b24a95e3-980c-4ba4-b027-2e43cb89ef3e.png", PdfFilePath = "/uploads/6fa80323-bb93-4ffb-8956-29bae2014bc7.pdf" },
                        new Book { Title = "Running a Racecar", Author = "Eliot J. Chen", Description = "Modern racing technology - the inside story", CategoryId = cat3.Id, CoverImagePath = "/uploads/32ebea11-bce1-4d92-957a-af330785122e.png", PdfFilePath = "/uploads/ac773fc4-bf28-4644-a2bd-4dc911bd542a.pdf" },
                        new Book { Title = "FORMULA 1 ALL THE RACES THE FIRST 1000", Author = "Roger Smith", Description = "Whether driver, car, team, race or championship, \r\nFormula 1 fans everywhere relish the GOAT \r\ndebate, 'Greatest Of All Time'. A book celebrating \r\n1000 Races could hardly dodge the issue; indeed isn't \r\nthis the perfect moment to shine a light?", CategoryId = cat4.Id, CoverImagePath = "/uploads/32191526-009f-40e5-a36c-433b925e6edb.png", PdfFilePath = "/uploads/c918399f-9d61-4dbb-a2b9-5a290a0aa4cb.pdf" },
                        new Book { Title = "Fifty Famous Motor Races", Author = "Alan Henry", Description = "Do you remember the spectacular tyre failure \r\nthat cost Nigel Mansell the World Champion\r\nship in the 1986 Australian Grand Prix? Or Niki \r\nlauda taking his third world title in Portugal two \r\nyears earlier from his McLaren team-mate \r\nAlain Prost by the wafer-thin margin of half a \r\npoint? These are just two of the exciting \r\nmoments recalled by Alan Henry in this action- \r\npacked account of the world's most dramatic \r\nraces. Many are etched into racing folklore and \r\nwill be recalled instantly by motor racing fans \r\neverywhere.", CategoryId = cat4.Id, CoverImagePath = "/uploads/6a60ba0c-e2a9-4b12-83c9-bed635aa21c1.png", PdfFilePath = "/uploads/732315ee-20ca-425a-a745-006839031e6b.pdf" },
                        new Book { Title = "F1 Racing Magazine", Author = "Haymarket", Description = "The ultimate magazine for gearheads and speed seekers.", CategoryId = cat2.Id, CoverImagePath = "/uploads/d050ce0d-f9c7-4f67-8a3f-128b4e66b358.png", PdfFilePath = "/uploads/5bbba35e-5e48-45e5-85ab-7cfb01e7769a.pdf" }
                    );
                    await dbContext.SaveChangesAsync();
                }
            }

            var testUserFromDb = await userManager.FindByEmailAsync("test@racereader.com");
            var adminUserFromDb = await userManager.FindByEmailAsync("admin@racereader.com");

            if (testUserFromDb != null && adminUserFromDb != null)
            {
                var books = dbContext.Books.ToList();
                foreach (var book in books)
                {
                    if (!dbContext.Comments.Any(c => c.BookId == book.Id))
                    {
                        dbContext.Comments.AddRange(
                            new Comment { BookId = book.Id, UserId = testUserFromDb.Id, Text = "Great book, highly recommend it! An absolute must-read for any motorsport fan.", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                            new Comment { BookId = book.Id, UserId = adminUserFromDb.Id, Text = "Found it a bit technical in some chapters, but overall very interesting.", CreatedAt = DateTime.UtcNow.AddDays(-1) }
                        );
                    }

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
