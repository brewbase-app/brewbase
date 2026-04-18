using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class UserRecipeFavorite
{
    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
