using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class RecipeRating
{
    public int Id { get; set; }

    public int Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
