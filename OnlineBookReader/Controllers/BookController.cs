using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookReader.Models;

namespace OnlineBookReader.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public BookController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Chapters.OrderBy(c => c.OrderNumber))
                .FirstOrDefaultAsync(m => m.Id.Equals(id));

            if (book == null)
            {
                return NotFound();
            }

            var recommendedBooks = await _context.Books
                .Where(b => b.CategoryId.Equals(book.CategoryId) &&
                            b.AuthorId.Equals(book.AuthorId) &&
                            !b.Id.Equals(book.Id))
                .OrderBy(b => Guid.NewGuid())
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Take(4)
                .ToListAsync();

            var existingIds = recommendedBooks.Select(b => b.Id).ToHashSet();

            if (recommendedBooks.Count < 4)
            {
                var remainingBooks = await _context.Books
                    .Where(b => b.CategoryId.Equals(book.CategoryId) &&
                                !b.Id.Equals(book.Id) &&
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
                var otherBooks = await _context.Books
                    .Where(b => !b.Id.Equals(book.Id) &&
                                !existingIds.Contains(b.Id))
                    .OrderBy(b => Guid.NewGuid())
                    .Include(b => b.Author)
                    .Include(b => b.Category)
                    .Take(4 - recommendedBooks.Count)
                    .ToListAsync();

                recommendedBooks.AddRange(otherBooks);
            }
            ViewData["RecommendedBooks"] = recommendedBooks;
            return View(book);
        }

        public async Task<IActionResult> ReadChapter(Guid chapterId)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Book)
                .Include(c => c.ChapterContents)
                .Include(c => c.ChapterImages.OrderBy(img => img.PageOrder))
                .FirstOrDefaultAsync(c => c.Id.Equals(chapterId));

            if (chapter == null)
            {
                return NotFound();
            }

            _ = Task.Run(async () =>
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var user = await db.Users.FindAsync(Guid.Parse(userId));
                    if (user != null)
                    {
                        user.LastReadId = chapter.Book.Id;
                        db.Users.Update(user);
                        var book = await db.Books.FindAsync(chapter.Book.Id);
                        if (book != null)
                        {
                            book.NumberRead++;
                            db.Books.Update(book);
                        }
                        await db.SaveChangesAsync();
                    }
                }
            });
            return View(chapter);
        }
    }
}
