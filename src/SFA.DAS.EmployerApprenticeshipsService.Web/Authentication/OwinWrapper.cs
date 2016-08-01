﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using Microsoft.Owin;
using SFA.DAS.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Authentication
{
    public class OwinWrapper : IOwinWrapper
    {
        private readonly IOwinContext _owinContext;
        private readonly EmployerApprenticeshipsServiceConfiguration _configuration;

        public OwinWrapper(EmployerApprenticeshipsServiceConfiguration configuration)
        {
            _configuration = configuration;
            _owinContext = HttpContext.Current.GetOwinContext();
           
        }

        public void SignInUser(string id, string displayName, string email)
        {
            if (!_configuration.Identity.UseFake) { throw new NotImplementedException(); }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, email),
                new Claim("sub", id)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            var authenticationManager = _owinContext.Authentication;
            authenticationManager.SignIn(claimsIdentity);
            _owinContext.Authentication.User = new ClaimsPrincipal(claimsIdentity);
        }

        public ActionResult SignOutUser()
        {
            if (_configuration.Identity.UseFake)
            {
                var authenticationManager = _owinContext.Authentication;
                authenticationManager.SignOut("Cookies");
                return new RedirectResult("/");
            }
            else
            {
                var authenticationManager = _owinContext.Authentication;
                authenticationManager.SignOut("Cookies");
                return new RedirectResult($"{_configuration.Identity.BaseAddress}/Login/dialog/appl/sfa/wflow/logout?returnUrl={_owinContext.Request.Uri.Scheme}://{_owinContext.Request.Uri.Authority}");
            }
        }

        public string GetClaimValue(string claimKey)
        {
            return ((ClaimsIdentity)HttpContext.Current.User.Identity).Claims.FirstOrDefault(claim => claim.Type == claimKey).Value;
        }

        public SignInMessage GetSignInMessage(string id)
        {
            return _owinContext.Environment.GetSignInMessage(id);
        }
        public void IssueLoginCookie(string id, string displayName)
        {
            //var env = _owinContext.Environment;
            //env.IssueLoginCookie(new AuthenticatedLogin
            //{
            //    Subject = id,
            //    Name = displayName
            //});
        }
        public void RemovePartialLoginCookie()
        {
            //_owinContext.Environment.RemovePartialLoginCookie();
        }
    }
}