using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblProduct
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public int? UnitId { get; set; }

    public string? UnitName { get; set; }

    public decimal? Proportion { get; set; }

    public double? PriceInput { get; set; }

    public double? PriceOutput { get; set; }

    public int? TypeProduct { get; set; }

    public int? CompanyId { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
}
