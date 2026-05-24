using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Report
{
    public int Id { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int ArticleId { get; set; }

    public int ReportedByUserId { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual AppUser ReportedByUser { get; set; } = null!;
}
