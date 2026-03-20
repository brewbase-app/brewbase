using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class CoffeeRating
{
    public int Id { get; set; }

    public int Value { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UserId { get; set; }

    public int CoffeeId { get; set; }

    public virtual Coffee Coffee { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
