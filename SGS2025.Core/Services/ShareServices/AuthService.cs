using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025.Core.Services.ShareServices
{
    public class AuthService
    {
        public async Task<bool> LoginAsync(string user, string pass)
        {
            // TODO: Gọi API check login
            if (user == "admin" && pass == "123")
            {
              //  await SecureStorage.SetAsync("auth_token", "dummy-token");
                return true;
            }
            return false;
        }

        public async Task<bool> HasValidTokenAsync()
        {
            //  var token = await SecureStorage.GetAsync("auth_token");
            //  return !string.IsNullOrEmpty(token);
            return false;
        }

        public async Task LogoutAsync()
        {
           // SecureStorage.Remove("auth_token");
        }
    }
}
