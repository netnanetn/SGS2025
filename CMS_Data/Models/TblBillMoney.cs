using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblBillMoney
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? ScaleId { get; set; }

    /// <summary>
    /// Hình thức thu, chi
    /// </summary>
    public int? TypeId { get; set; }

    public int? TypeCate { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerPhone { get; set; }

    public double? Money { get; set; }

    public string? Note { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
}
