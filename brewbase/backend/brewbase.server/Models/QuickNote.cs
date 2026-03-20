using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class QuickNote
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
