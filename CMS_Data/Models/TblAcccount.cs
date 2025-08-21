using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblAcccount
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? PassWord { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
}
