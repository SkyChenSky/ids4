using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sikiro.ClientB.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult Logout()
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
                return SignOut("Cookies", "oidc");

            return RedirectToAction("Index", "Home");
        }
    }
}
