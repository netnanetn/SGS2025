using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.ModelDTO
{
    public class ScaleFilterDTO
    {
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public bool XeChoCanLan2 { get; set; }
        public bool XeDaCan { get; set; }
        public string? KieuCan { get; set; } = "Tất cả";
        public string? TuKhoa { get; set; } // nếu sau này bạn muốn thêm ô tìm kiếm
    }
    public class ScaleStatisticDTO
    {
        public int SoLan1TrongNgay { get; set; }
        public int SoLan2TrongNgay { get; set; }
        public int SoXeChoCanLan2 { get; set; }
    }
}
