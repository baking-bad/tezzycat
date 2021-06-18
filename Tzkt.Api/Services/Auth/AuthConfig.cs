using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tzkt.Api.Services.Auth
{
    public class AuthConfig
    {
        public AuthMethod Method { get; set; } = AuthMethod.None;
        public int NonceLifetime { get; set; } = 10;
        public Dictionary<string, string> Credentials { get; set; } = new();
    }

    public static class AuthConfigExt
    {
        public static AuthConfig GetAuthConfig(this IConfiguration config)
        {
            return config.GetSection("Authentication")?.Get<AuthConfig>();
        }
    }
}