using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Herbology.Materials
{
	public class Ivy : QuickMaterial
    {
        public Ivy() : base("Forest Ivy", "A common, yet versatile herb", 999, 100, 1, AssetDirectory.BrewingItem) { }
    }

    public class IvySeeds : QuickMaterial
    {
        public IvySeeds() : base("Forest Ivy Seeds", "Can grow in hanging planters", 99, 0, 1, AssetDirectory.BrewingItem) { }
    }
}