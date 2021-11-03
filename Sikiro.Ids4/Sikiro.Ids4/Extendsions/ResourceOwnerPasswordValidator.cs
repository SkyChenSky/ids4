using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Sikiro.Ids4.Services;

namespace Sikiro.Ids4.Extendsions
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private UserService _userService;
        public ResourceOwnerPasswordValidator(UserService userService)
        {
            _userService = userService;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = _userService.GetUser(context.UserName, context.Password);
            if (user != null)
            {
                var userClaims = new[]
                {
                    new Claim("UserId", user.UserId),
                    new Claim(JwtClaimTypes.Name, user.UserName),
                    new Claim(JwtClaimTypes.PhoneNumber, user.Phone)
                };

                context.Result = new GrantValidationResult(
                    user.UserId,
                    OidcConstants.AuthenticationMethods.Password,
                    userClaims);
            }
            else
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "账号与密码不匹配");

            return Task.CompletedTask;
        }
    }
}
