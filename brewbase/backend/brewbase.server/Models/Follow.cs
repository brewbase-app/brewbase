using System;
using System.Collections.Generic;

namespace brewbase.server.Models;

public partial class Follow
{
    public int FollowerId { get; set; }

    public int FollowedId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppUser Followed { get; set; } = null!;

    public virtual AppUser Follower { get; set; } = null!;
}
