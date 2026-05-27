using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class UserCoffeeFavorite
{
    public int UserId { get; set; }

    public int CoffeeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Coffee Coffee { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
