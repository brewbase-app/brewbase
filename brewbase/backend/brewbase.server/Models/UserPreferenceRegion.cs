using brewbase.server.Models;

namespace DefaultNamespace;

public partial class UserPreferenceRegion
{
    public int UserPreferenceId { get; set; }

    public int RegionId { get; set; }

    public virtual UserPreference UserPreference { get; set; } = null!;

    public virtual Region Region { get; set; } = null!;
}