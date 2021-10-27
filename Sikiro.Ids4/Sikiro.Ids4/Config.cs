using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Sikiro.Ids4
{
    public static class Config
    {
        public static IEnumerable<ApiResource> GetApiResource()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "api",new List<string>{ClaimTypes.Role })
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static List<TestUser> GetTestUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "chengong",
                    Password = "123456",
                    Claims = new[]
                    {
                        new Claim(ClaimTypes.Role, "超级管理员")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "chengong2",
                    Password = "123456",
                    Claims = new[]
                    {
                        new Claim(ClaimTypes.Role, "普通用户")
                    }
                }
            };
        }

        public static List<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "web",
                    ClientName = "web_client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                   // RequireConsent = false,//设置是否要授权页面
                    RedirectUris = { "http://www.clienta.com:5001/signin-oidc","http://www.clientb.com:5002/signin-oidc"},
                    PostLogoutRedirectUris = {"http://www.clienta.com:5001/signout-callback-oidc","http://www.clientb.com:5002/signout-callback-oidc"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },
                    AllowOfflineAccess=true
                }
            };
        }
    }
}
