using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AspNetIdentityFromScratch.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {            
            return View();
        }

        [Authorize(Roles="Administrator")]
        public IActionResult RequiresAdmin()
        {
            return Content("OK");
        }
    }
}
