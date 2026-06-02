namespace DefaultNamespace;

public partial class FlavorProfile
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<UserPreferenceFlavorProfile> UserPreferenceFlavorProfiles { get; set; } = new List<UserPreferenceFlavorProfile>();
    public virtual ICollection<CoffeeFlavorProfile> CoffeeFlavorProfiles { get; set; } = new List<CoffeeFlavorProfile>();
}