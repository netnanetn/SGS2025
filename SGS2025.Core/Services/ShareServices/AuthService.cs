using CMS_Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025.Core.Services.ShareServices
{
    public class AuthService
    { 
        private AccountService _accountService;
        public AuthService(AccountService accountService) {
         _accountService = accountService;
        }
        public async Task<bool> LoginAsync(string user, string pass)
        {
            var bamtest = UtilsService.HashPassword(pass);
            // TODO: Gọi API check login
            var acccount = await _accountService.CheckLoginAsync(user, pass);
            if (acccount != null) {
                //  await SecureStorage.SetAsync("auth_token", "dummy-token"); 
                return true;
            }
           
            return false;
        }
        public async Task<bool> LoginByHashAsync(string user, string hashed)
        { 
            // TODO: Gọi API check login
            var acccount = await _accountService.CheckLoginAsync(user, hashed);
            if (acccount != null)
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
