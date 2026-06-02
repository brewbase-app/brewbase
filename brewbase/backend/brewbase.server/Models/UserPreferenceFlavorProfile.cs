using brewbase.server.Models;

namespace DefaultNamespace;

public partial class UserPreferenceFlavorProfile
{
    public int UserPreferenceId { get; set; }

    public int FlavorProfileId { get; set; }

    public virtual UserPreference UserPreference { get; set; } = null!;

    public virtual FlavorProfile FlavorProfile { get; set; } = null!;
}