using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4.Services;
using IdentityServer4.Test;
using IdentityServer4;
using Sikiro.Ids4.Filer;
using Sikiro.Ids4.Models;

namespace Sikiro.Ids4.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(IIdentityServerInteractionService interaction)
        {
            _users = new TestUserStore(Config.GetTestUsers());
            _interaction = interaction;
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
            if (_users.FindByUsername(model.Username) == null ||
                !_users.ValidateCredentials(model.Username, model.Password))
            {
                model.ErrorMessage = "登录验证不通过";
                return View(model);
            }

            //使用IdentityServer的SignInAsync登录
            await HttpContext.SignInAsync(new IdentityServerUser(model.Username), new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddDays(7),
                IsPersistent = true,
                AllowRefresh = true
            });

            //验证ReturnUrl是否可以使用
            if (_interaction.IsValidReturnUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
