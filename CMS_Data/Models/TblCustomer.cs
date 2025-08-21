using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblCustomer
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public double? DebtFirst { get; set; }

    public string? BankAccount { get; set; }

    public string? Note { get; set; }

    public int? CompanyId { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
}
