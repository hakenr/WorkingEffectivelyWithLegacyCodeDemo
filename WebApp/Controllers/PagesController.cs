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
	    private readonly IEmailHelper emailHelper;

	    public PagesController(IEmailHelper emailHelper)
	    {
		    this.emailHelper = emailHelper;
	    }

        // GET: Pages
        public ActionResult Index()
        {
            return View();
        }

	    public ActionResult SendMail()
	    {
		    emailHelper.Enqueue("haken@havit.cz", "MigrosChester EmailHelper TEST", "Fungujeee...!");

			return View("Index");
	    }
    }
}