using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Article
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public int? ModeratedByUserId { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public string? ModerationComment { get; set; }

    public int UserId { get; set; }

    public virtual AppUser? ModeratedByUser { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
