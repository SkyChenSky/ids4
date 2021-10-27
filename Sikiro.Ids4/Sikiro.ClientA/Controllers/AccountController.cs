using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sikiro.ClientA.Controllers
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
