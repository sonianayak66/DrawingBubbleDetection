using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MPCRS.Controllers
{
    [Authorize]
    public class TaskManagerController : Controller
    {
        private readonly IConfiguration _configuration;

        public TaskManagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var taskManagerUrl = _configuration["AppSettings:TaskManagerUrl"];
            ViewData["TaskManagerUrl"] = taskManagerUrl;
            return View();
        }
    }
}
