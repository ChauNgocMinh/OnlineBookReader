using Microsoft.AspNetCore.Mvc;

namespace OnlineBookReader.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ViewResult Index()
        {
            return View();
        }
    }
}
