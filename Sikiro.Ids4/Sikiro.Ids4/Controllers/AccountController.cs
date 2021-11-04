using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4.Services;
using IdentityServer4.Test;
using IdentityServer4;
using IdentityServer4.Events;
using Sikiro.Ids4.Models;
using Sikiro.Ids4.Services;

namespace Sikiro.Ids4.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IEventService events, UserService userService)
        {
            _interaction = interaction;
            _events = events;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                if (_interaction.IsValidReturnUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            var model = new LoginInputModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            var user = _userService.GetUser(model.Username, model.Password);
            if (user != null)
            {
                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.UserId, user.UserName, clientId: context?.Client.ClientId));

                //使用IdentityServer的SignInAsync登录
                await HttpContext.SignInAsync(new IdentityServerUser(user.UserId) { DisplayName = user.UserName,AdditionalClaims = new List<Claim>
                {
                    new Claim("UserId", user.UserId),
                    new Claim("UserName", user.UserName),
                    new Claim("PhoneNumber", user.Phone)
                }
                }, new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    IsPersistent = true,
                    AllowRefresh = true
                });

                if (Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return Redirect("~/");

                throw new Exception("invalid return URL");
            }

            model.ErrorMessage = "账号不匹配";

            return View(model);
        }

        public async Task<IActionResult> Logout(string logoutId)
        {
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                await HttpContext.SignOutAsync();

            if (logout.PostLogoutRedirectUri != null)
                return Redirect(logout.PostLogoutRedirectUri);

            return RedirectToAction("Index", "Home");
        }
    }
}
