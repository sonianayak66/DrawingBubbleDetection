using Microsoft.AspNetCore.Mvc;

namespace MPCRS.Controllers
{
	public class ErrorController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
