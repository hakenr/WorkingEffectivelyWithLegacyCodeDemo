using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class PagesController : Controller
    {
        // GET: Pages
        public ActionResult Index()
        {
            return View();
        }
    }
}