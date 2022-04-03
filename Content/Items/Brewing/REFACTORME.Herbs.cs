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
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Green;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.consumable = true;
            Item.createTile = Mod.TileType("Deathstalk");
        }
    }
}