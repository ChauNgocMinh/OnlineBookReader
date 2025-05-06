using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookReader.Models;

namespace OnlineBookReader.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public BookController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index(string searchTerm, int page = 1)
        {
            int pageSize = 10;

            var books = _context.Books
                .Where(b => !b.IsDeleted)
                .Where(b => string.IsNullOrEmpty(searchTerm) || EF.Functions.Like(b.Title, "%" + searchTerm + "%"))
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt);

            var totalItems = await books.CountAsync();
            var items = await books.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(items);
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
