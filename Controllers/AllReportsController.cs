using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MPCRS.Controllers
{
    [Authorize]
    public class AllReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}