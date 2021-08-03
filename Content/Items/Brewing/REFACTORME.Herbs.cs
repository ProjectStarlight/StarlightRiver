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

    public class Deathstalk : ModItem
    {
        public override string Texture => AssetDirectory.BrewingItem + Name;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Grows on Rich Soil");
            DisplayName.SetDefault("Deathstalk");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.rare = ItemRarityID.Green;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createTile = mod.TileType("Deathstalk");
        }
    }
}