using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using RaceReader.Data;
using RaceReader.Models;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace RaceReader.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateCheckoutSession(int tokens, long price, string packageName)
        {
            var domain = $"{Request.Scheme}://{Request.Host}";
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            var options = new SessionCreateOptions
            {
                Locale = "en",
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = price,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"{packageName} ({tokens} Tokens)",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Home/Pricing",
                ClientReferenceId = userId,
                Metadata = new Dictionary<string, string>
                {
                    { "Tokens", tokens.ToString() },
                    { "UserId", userId }
                }
            };

            var service = new SessionService();
            
            try 
            {
                var session = service.Create(options);
                return Redirect(session.Url);
            }
            catch(Exception ex)
            {
                // Если ключ недействителен или отсутствует, выводим ошибку, а не имитируем
                return BadRequest($"Stripe error: {ex.Message}");
            }
        }

        [Authorize]
        public async Task<IActionResult> SimulateSuccess(int tokens, long amount)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            var user = await _context.Users.FindAsync(userId);
            
            if (user != null)
            {
                user.TokenBalance += tokens;
                
                _context.Transactions.Add(new Transaction
                {
                    UserId = userId,
                    StripeSessionId = "simulated_" + Guid.NewGuid().ToString(),
                    Amount = amount / 100m,
                    TokensAdded = tokens,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                });
                
                await _context.SaveChangesAsync();
            }
            
            return View("Success");
        }

        public async Task<IActionResult> Success(string session_id)
        {
            if (!string.IsNullOrEmpty(session_id))
            {
                try
                {
                    var service = new SessionService();
                    var session = service.Get(session_id);

                    if (session != null && session.PaymentStatus == "paid")
                    {
                        var existingTx = await _context.Transactions.FirstOrDefaultAsync(t => t.StripeSessionId == session.Id);
                        
                        // Если вебхук еще не успел обработать или не дошел (например, на localhost)
                        if (existingTx == null)
                        {
                            var userId = session.Metadata["UserId"];
                            var tokens = int.Parse(session.Metadata["Tokens"]);
                            
                            var user = await _context.Users.FindAsync(userId);
                            if (user != null)
                            {
                                user.TokenBalance += tokens;
                                
                                _context.Transactions.Add(new Transaction
                                {
                                    UserId = userId,
                                    StripeSessionId = session.Id,
                                    Amount = (session.AmountTotal ?? 0) / 100m,
                                    TokensAdded = tokens,
                                    Status = "Completed",
                                    CreatedAt = DateTime.UtcNow
                                });
                                
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логируем или игнорируем ошибку получения сессии, 
                    // чтобы не пугать пользователя на странице успеха
                }
            }
            
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _configuration["Stripe:WebhookSecret"]);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    
                    if (session != null)
                    {
                        var userId = session.Metadata["UserId"];
                        var tokens = int.Parse(session.Metadata["Tokens"]);
                        
                        var user = await _context.Users.FindAsync(userId);
                        if (user != null)
                        {
                            user.TokenBalance += tokens;
                            
                            _context.Transactions.Add(new Transaction
                            {
                                UserId = userId,
                                StripeSessionId = session.Id,
                                Amount = (session.AmountTotal ?? 0) / 100m,
                                TokensAdded = tokens,
                                Status = "Completed",
                                CreatedAt = DateTime.UtcNow
                            });
                            
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}
