using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblVehicle
{
    public int Id { get; set; }

    public string VehiceCode { get; set; } = null!;

    public string? RfidCode { get; set; }

    public string? DriverName { get; set; }

    public string? DriverPhone { get; set; }

    public double? TonnageDefault { get; set; }

    public string? TransportationCompany { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
}
