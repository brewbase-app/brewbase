using brewbase.server.Models;

namespace DefaultNamespace;

public partial class UserPreferenceBrewingMethod
{
    public int UserPreferenceId { get; set; }

    public int BrewingMethodId { get; set; }

    public virtual UserPreference UserPreference { get; set; } = null!;

    public virtual BrewingMethod BrewingMethod { get; set; } = null!;
}