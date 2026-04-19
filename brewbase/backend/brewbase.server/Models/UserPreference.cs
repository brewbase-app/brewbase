using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class UserPreference
{
    public int Id { get; set; }

    public string PreferredRoastLevel { get; set; } = null!;

    public string FavoriteNotes { get; set; } = null!;

    public bool QuizCompleted { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public virtual AppUser User { get; set; } = null!;
}
