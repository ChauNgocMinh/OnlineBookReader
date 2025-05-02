using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookReader.Models;

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
            var books = await _dbContext.Books.Take(4).ToListAsync();
            return View(books);
        }
    }
}
