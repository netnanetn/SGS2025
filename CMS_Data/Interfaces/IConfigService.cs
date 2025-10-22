using CMS_Data.ModelDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Interfaces
{
    public interface IConfigService
    {
        Task<AppConfig> LoadAsync();
        Task SaveAsync(AppConfig config);
        AppConfig GetConfig();
    }
}
