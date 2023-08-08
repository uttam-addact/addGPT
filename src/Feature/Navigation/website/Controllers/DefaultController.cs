using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lawfirm.Feature.Navigation.Models;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;

namespace Lawfirm.Feature.Navigation.Controllers
{
    public class DefaultController : Controller
    {
        public ActionResult booklist(string search = "")
        {
            if (search != null)
            {
                Database db = Sitecore.Context.Item.Database;
                List<Item> applications = db.GetItem("/sitecore/content/Sites/Main/Home/books").GetChildren().ToList();
                var searchResult = applications.Where(x => x.Name.ToLower().Contains(search.ToLower()) || x.Fields["PageTitle"].Value.ToLower().Contains(search.ToLower()) || x.Fields["DescriptionMore"].Value.ToLower().Contains(search.ToLower())).ToList();

                if (searchResult != null)
                {
                    return View("~/Views/Default/booklist.cshtml", searchResult);
                }
                else
                {
                    return View("~/Views/Default/booklist.cshtml", applications);
                }
            }
            else
            {
                Database db = Sitecore.Context.Item.Database;
                List<Item> applications = db.GetItem("/sitecore/content/Sites/Main/Home/books").GetChildren().ToList();
                return View("~/Views/Default/booklist.cshtml", applications);
            }
        }
        public ActionResult Login()
        {
            return View("~/Views/Default/login.cshtml");
        }

        [HttpPost]
        public ActionResult Login(UserModel userModel)
        {
            var domain = Sitecore.Context.Domain;

            if (domain != null)
            {
                userModel.AccountName = domain + "\\" + userModel.UserName;
            }
            var sitecoreuser = GetUserbyLoginName(userModel.AccountName);

            var result = AuthenticationManager.Login(userModel.AccountName, userModel.Password, true);

            var activate = AuthenticationManager.GetActiveUser();
            if (sitecoreuser != null && result != false)
            {
                string queryString = Request.QueryString["query"];
                if (!string.IsNullOrEmpty(queryString) && queryString != "/login")
                {
                    return this.Redirect(queryString); //View("~/Views/Default/BooklList.cshtml");
                }
                else
                {
                    return this.Redirect("/books"); //View("~/Views/Default/BooklList.cshtml");
                }
            }
            else if (sitecoreuser == null)
            {
                ViewBag.message = "User does not exist";
            }
            else if (result == false)
            {
                ViewBag.message = "please enter correct credentials";
            }
            else
            {
                ViewBag.message = "Oops!! something went wrong";
                return this.View("~/Views/Default/login.cshtml", userModel);
            }
            return this.View("~/Views/Default/login.cshtml", userModel);
        }

        public User GetUserbyLoginName(string userName)
        {
            var UserName = userName;

            if (Sitecore.Security.Accounts.User.Exists(UserName))
                return Sitecore.Security.Accounts.User.FromName(UserName, true);
            return null;
        }
    }
}
