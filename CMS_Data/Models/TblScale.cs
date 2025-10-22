using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblScale
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Vehicle { get; set; }

    public string? DriverName { get; set; }

    public DateTime? TimeIn { get; set; }

    public decimal? WeightIn { get; set; }

    public DateTime? TimeOut { get; set; }

    public decimal? WeightOut { get; set; }

    public decimal? WeightImpurity { get; set; }

    public decimal? ProductNumber { get; set; }

    public string? ProductName { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerPhone { get; set; }

    public int? ProductId { get; set; }

    public double? ProductPrice { get; set; }

    public double? ProductMoney { get; set; }

    public double? Proportion { get; set; }

    public decimal? Exchange { get; set; }

    public string? Note { get; set; }

    public int? TypeId { get; set; }

    public string? TopSeal { get; set; }

    public bool? IsActive { get; set; }

    public double? TotalMoney { get; set; }

    public double? MoneyPayment { get; set; }

    public double? MoneyDiscount { get; set; }

    public double? MoneyDebt { get; set; }

    public string? StatusPayment { get; set; }

    public string? Img11 { get; set; }

    public string? Img12 { get; set; }

    public string? Img13 { get; set; }

    public string? Img21 { get; set; }

    public string? Img22 { get; set; }

    public string? Img23 { get; set; }

    public int? IndexInDay { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
    public int? Status { get; set; }
    public int? SyncStatus { get; set; }
    public DateTime? SyncTime { get; set; }
    public int? SyncFailCount { get; set; }
    public string? SyncError { get; set; }
    public int? ServerId { get; set; }
}
