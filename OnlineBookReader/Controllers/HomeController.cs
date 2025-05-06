using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBookReader.Models;
using System.Security.Claims;

namespace OnlineBookReader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public HomeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ViewResult> Index()
        {
            var books = await _dbContext.Books.ToListAsync();

            List<Book> recommendedBooks = new();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

                if (user?.LastReadId is Guid lastReadId && lastReadId != Guid.Empty)
                {
                    var book = await _dbContext.Books
                        .Include(b => b.Author)
                        .Include(b => b.Category)
                        .FirstOrDefaultAsync(b => b.Id.Equals(user.LastReadId));

                    if (book != null)
                    {
                        recommendedBooks = await _dbContext.Books
                            .Where(b => b.CategoryId == book.CategoryId &&
                                        b.AuthorId == book.AuthorId &&
                                        b.Id != book.Id)
                            .OrderBy(b => Guid.NewGuid())
                            .Include(b => b.Author)
                            .Include(b => b.Category)
                            .Take(4)
                            .ToListAsync();

                        var existingIds = recommendedBooks.Select(b => b.Id).ToHashSet();

                        if (recommendedBooks.Count < 4)
                        {
                            var remainingBooks = await _dbContext.Books
                                .Where(b => b.CategoryId == book.CategoryId &&
                                            b.Id != book.Id &&
                                            !existingIds.Contains(b.Id))
                                .OrderBy(b => Guid.NewGuid())
                                .Include(b => b.Author)
                                .Include(b => b.Category)
                                .Take(4 - recommendedBooks.Count)
                                .ToListAsync();

                            recommendedBooks.AddRange(remainingBooks);
                            existingIds.UnionWith(remainingBooks.Select(b => b.Id));
                        }

                        if (recommendedBooks.Count < 4)
                        {
                            var otherBooks = await _dbContext.Books
                                .Where(b => b.Id != book.Id &&
                                            !existingIds.Contains(b.Id))
                                .OrderBy(b => Guid.NewGuid())
                                .Include(b => b.Author)
                                .Include(b => b.Category)
                                .Take(4 - recommendedBooks.Count)
                                .ToListAsync();

                            recommendedBooks.AddRange(otherBooks);
                        }
                    }
                }
                else
                {
                    recommendedBooks = await _dbContext.Books
                        .OrderBy(b => Guid.NewGuid())
                        .Include(b => b.Author)
                        .Include(b => b.Category)
                        .Take(4)
                        .ToListAsync();
                }
            }

            ViewData["RecommendedBooks"] = recommendedBooks;
            return View(books);
        }

        public async Task<ViewResult> ViewAllBooks(string search, bool isOrderBY)
        {
            var query = _dbContext.Books
                .Where(x => string.IsNullOrEmpty(search) || x.Title.ToLower().Contains(search.ToLower()));

            if (isOrderBY)
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            var books = await query.ToListAsync();
            return View(books);
        }
    }
}
