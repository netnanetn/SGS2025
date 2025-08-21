using System;
using System.Collections.Generic;

namespace CMS_Data.Models;

public partial class TblCategoryPayment
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Note { get; set; }

    /// <summary>
    /// 1: Thu; 2: Chi
    /// </summary>
    public int? TypeId { get; set; }

    public DateTime? CreateDay { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDay { get; set; }

    public string? UpdateBy { get; set; }
}
