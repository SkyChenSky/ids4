using System;
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

namespace Sikiro.Ids4.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IEventService events,
            TestUserStore users = null)
        {
            _users = users ?? new TestUserStore(Config.GetTestUsers());
            _interaction = interaction;
            _events = events;
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
            if (_users.ValidateCredentials(model.Username, model.Password))
            {
                var user = _users.FindByUsername(model.Username);

                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username, clientId: context?.Client.ClientId));

                //使用IdentityServer的SignInAsync登录
                await HttpContext.SignInAsync(new IdentityServerUser(user.SubjectId) { DisplayName = user.Username }, new AuthenticationProperties
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

            if(logout.PostLogoutRedirectUri != null)
                return Redirect(logout.PostLogoutRedirectUri);

            return RedirectToAction("Index", "Home");
        }
    }
}
