using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Enums
{
    public enum ScaleStatus
    {
        Weighing = 0,     // đang cân
        Completed = 1     // đã hoàn thành
    }

    public enum SyncStatus
    {
        NotSynced = 0,    // chưa gửi
        Synced = 1,       // đã gửi
        Failed = 2        // lỗi gửi
    }
}
