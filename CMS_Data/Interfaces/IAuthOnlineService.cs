using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Interfaces
{
    public interface IAuthOnlineService
    {
        Task<bool> LoginAsync(string username, string password);
        Task SaveTokensAsync(string access, string refresh);

        Task<string?> GetAccessTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task<bool> RefreshTokenAsync();
        void Logout();
    }
}
