using brewbase.server.Models;

namespace DefaultNamespace;

public partial class CoffeeFlavorProfile
{
    public int CoffeeId { get; set; }

    public int FlavorProfileId { get; set; }

    public virtual Coffee Coffee { get; set; } = null!;

    public virtual FlavorProfile FlavorProfile { get; set; } = null!;
}