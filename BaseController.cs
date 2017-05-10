using CommonTableFiels.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CommonTableFiels.Controllers
{
    public class BaseController : Controller
    {
        public Entities db { get; set; }

        public BaseController()
        {
            db = new Entities();
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            if(Request.IsAuthenticated)
            {
                var UserManager = HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>();

                var user = UserManager.FindById(User.Identity.GetUserId());
                if(user != null)
                {
                    db.UserId = user.Id;
                }
            }
            return base.BeginExecuteCore(callback, state);
        }

    }
}
