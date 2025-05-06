using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookReader.Models;
using OnlineBookReader.ViewModel;

namespace OnlineBookReader.Controllers.AdminSite
{
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
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                CategoryId = model.CategoryId,
                AuthorId = model.AuthorId,
                ShortDescription = model.ShortDescription,
                CreatedAt = DateTime.Now
            };
            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(model.CoverImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImageFile.CopyToAsync(stream);
                }
                book.UrlImage = "/image/" + fileName;
            }
            _context.Books.Add(book);

            foreach (var chapterVM in model.Chapters)
            {
                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id,
                    Title = chapterVM.Title,
                    OrderNumber = chapterVM.OrderNumber
                };
                _context.Chapters.Add(chapter);

                if (chapterVM.IsText && !string.IsNullOrWhiteSpace(chapterVM.HtmlContent))
                {
                    var content = new ChapterContent
                    {
                        Id = Guid.NewGuid(),
                        ChapterId = chapter.Id,
                        Content = chapterVM.HtmlContent
                    };
                    _context.ChapterContents.Add(content);
                }
                else if (chapterVM.Images != null)
                {
                    int page = 1;
                    foreach (var file in chapterVM.Images)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine("wwwroot/image", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var image = new ChapterImage
                        {
                            Id = Guid.NewGuid(),
                            ChapterId = chapter.Id,
                            ImageUrl = $"/image/{fileName}",
                            PageOrder = page++
                        };
                        _context.ChapterImages.Add(image);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Chapters)
                .ThenInclude(c => c.ChapterContents) 
                .Include(b => b.Chapters)
                .ThenInclude(c => c.ChapterImages)  
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var viewModel = new BookCreateViewModel
            {
                Id = book.Id,
                Title = book.Title,
                CategoryId = book.CategoryId,
                AuthorId = book.AuthorId,
                ShortDescription = book.ShortDescription,
                Chapters = book.Chapters.Select(c => new ChapterCreateViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    OrderNumber = c.OrderNumber,
                    IsText = c.ChapterContents.Any()  
                }).ToList(),
            };

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);

            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BookCreateViewModel model)
        {
            var book = await _context.Books
                   .Include(b => b.Chapters)          
                   .ThenInclude(c => c.ChapterContents)     
                   .Include(b => b.Chapters)           
                   .ThenInclude(c => c.ChapterImages)    
                   .FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            book.Title = model.Title;
            book.CategoryId = model.CategoryId;
            book.AuthorId = model.AuthorId;
            book.ShortDescription = model.ShortDescription;
            book.UpdatedAt = DateTime.Now;

            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(model.CoverImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImageFile.CopyToAsync(stream);
                }
                book.UrlImage = "/image/" + fileName;
            }

            foreach (var chapterVM in model.Chapters)
            {
                var chapter = book.Chapters.FirstOrDefault(c => c.Id == chapterVM.Id) ?? new Chapter
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id
                };

                chapter.Title = chapterVM.Title;
                chapter.OrderNumber = chapterVM.OrderNumber;

                if (chapter.Id == Guid.Empty)
                {
                    _context.Chapters.Add(chapter);
                }

                if (chapterVM.IsText && !string.IsNullOrWhiteSpace(chapterVM.HtmlContent))
                {
                    var content = chapter.ChapterContents.FirstOrDefault() ?? new ChapterContent
                    {
                        Id = Guid.NewGuid(),
                        ChapterId = chapter.Id,
                    };
                    content.Content = chapterVM.HtmlContent;
                    if (chapter.ChapterContents.Count == 0)
                    {
                        _context.ChapterContents.Add(content);
                    }
                }

                if (chapterVM.Images != null)
                {
                    int page = 1;
                    foreach (var file in chapterVM.Images)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine("wwwroot/image", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var image = new ChapterImage
                        {
                            Id = Guid.NewGuid(),
                            ChapterId = chapter.Id,
                            ImageUrl = $"/image/{fileName}",
                            PageOrder = page++
                        };
                        _context.ChapterImages.Add(image);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
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
