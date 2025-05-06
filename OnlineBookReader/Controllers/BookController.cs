using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Books.Include(b => b.Author).Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
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
                        await db.SaveChangesAsync();
                    }
                }
            });
            return View(chapter);
        }

        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,CategoryId,AuthorId,UrlImage,ShortDescription,Content,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,IsDeleted")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.Id = Guid.NewGuid();
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,CategoryId,AuthorId,UrlImage,ShortDescription,Content,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,IsDeleted")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(Guid id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
