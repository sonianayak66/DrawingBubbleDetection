using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using System;
using XAct;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using static MPCRS.Utilities.Constants;
using System.Collections.Generic;
using XAct.Library.Settings;

namespace MPCRS.Controllers
{
    public class ExcelUploadController : Controller
    {
        [Authorize]
        public IActionResult Index(string basetable)
        {
            ViewBag.Basetable = basetable;
            return PartialView("Index");
        }

      

    }
}
