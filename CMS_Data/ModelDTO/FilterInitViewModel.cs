using CMS_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.ModelDTO
{
    public class FilterInitViewModel
    {
        public List<TblProduct> Products { get; set; }
        public List<TblCustomer> Customers { get; set; }
        public List<TblVehicle> Vehicles { get; set; }

        public FilterInitViewModel()
        {
            Products = new List<TblProduct>();
            Customers = new List<TblCustomer>();
            Vehicles = new List<TblVehicle>();
        }
    }
}
