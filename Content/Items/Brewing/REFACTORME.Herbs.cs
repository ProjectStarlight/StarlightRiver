using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Herbology;

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

    public class ForestBerries : ModItem
    {
        public override string Texture => AssetDirectory.BrewingItem + Name;

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.consumable = true;
            item.maxStack = 99;
            item.useTime = 15;
            item.useAnimation = 15;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.healLife = 5;
            item.potion = true;
            item.UseSound = SoundID.Item2;
        }

        public override bool UseItem(Player player)
        {
            player.AddBuff(BuffID.PotionSickness, 15);
            return true;
        }
    }

    public class BerryBush : QuickTileItem
    {
        public BerryBush() : base("Berry bush", "Plant to grow your own berries!", TileType<ForestBerryBush>(), 1, AssetDirectory.BrewingItem) { }
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