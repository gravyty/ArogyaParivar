using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArogyaParivar.Controllers
{
    public class ChatController : Controller
    {
        //
        // GET: /Chat/

        public ActionResult Index()
        {
            string rand = new Random().Next(10000).ToString();
            while (Hubs.OnlineUser.userObj.Where(item => item.userId == rand).Count() > 0)
            {
                rand = new Random().Next(10000).ToString();
            }
            if (Hubs.OnlineUser.userObj.Where(item => item.sessionId == this.HttpContext.Session.SessionID.ToString()).Count() > 0)
                Hubs.OnlineUser.userObj.Remove(Hubs.OnlineUser.userObj.Where(item => item.sessionId == this.HttpContext.Session.SessionID.ToString()).FirstOrDefault());
            Hubs.UserModal thisUser = Hubs.OnlineUser.AddOnlineUser("", "U" + rand , rand, this.HttpContext.Session.SessionID.ToString());

            return View("~/Views/Chat/Chat.cshtml",thisUser);
        }

    }
}
