using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Common;
using Sitecore.Security;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
using Sitecore.Web.Authentication;

namespace Lawfirm.Feature.Navigation.Service
{
    public static class UserRepository
    {
        public static User GetUser(string userName, string password)
        {
            var domain = Sitecore.Context.Domain;
            if (!System.Web.Security.Membership.ValidateUser(domain + @"\" + userName, password))
                return null;
            if (User.Exists(domain + @"\" + userName))
                return User.FromName(domain + @"\" + userName, true);
            return null;
        }
        public static bool Login(string userName, string password)
        {
            var domain = Sitecore.Context.Domain;

            return AuthenticationManager.Login(domain + @"\" + userName, password, false);
        }
    }
}