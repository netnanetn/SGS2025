using CMS_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.ModelDTO
{
    public class PrintScaleDTO
    {
        public TblScale Scale { get; set; }
        public TblCompany Company { get; set; }
        public PrintScaleDTO() {
            Scale = new TblScale();
            Company = new TblCompany();
        }
    }
}
