using Microsoft.AspNetCore.Mvc;

namespace eKitap.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return Content("test");
        }
    }
}
