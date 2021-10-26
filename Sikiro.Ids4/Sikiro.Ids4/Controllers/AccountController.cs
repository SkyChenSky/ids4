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
using IdentityServer4.Stores;
using Sikiro.Ids4.Filer;
using Sikiro.Ids4.Models;

namespace Sikiro.Ids4.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            _users = users ?? new TestUserStore(Config.GetTestUsers());

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
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

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // validate username/password against in-memory store
            if (_users.ValidateCredentials(model.Username, model.Password))
            {
                var user = _users.FindByUsername(model.Username);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username, clientId: context?.Client.ClientId));

                //使用IdentityServer的SignInAsync登录
                await HttpContext.SignInAsync(new IdentityServerUser(user.SubjectId) { DisplayName = user.Username }, new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    IsPersistent = true,
                    AllowRefresh = true
                });

                // request for a local page
                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new Exception("invalid return URL");
                }
            }

            model.ErrorMessage = "账号不匹配";

            return View(model);
        }
    }
}
