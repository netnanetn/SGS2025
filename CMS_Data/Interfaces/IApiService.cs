using CMS_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Interfaces
{
    public interface IApiService
    {
        Task<(bool Success, int ServerId, string? Error)> SendRecordAsync(TblScale record);
    }
}
