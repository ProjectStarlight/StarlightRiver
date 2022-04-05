using StarlightRiver.Content.Tiles.Herbology;
using StarlightRiver.Items.Herbology.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Herbology
{
	public class Soil : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/Soil";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rich Soil");
            Tooltip.SetDefault("Used to grow exotic herbs");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<SoilTile>();
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(10);
            recipe.AddIngredient(ItemID.MudBlock, 10);
            recipe.AddIngredient(ItemType<Ivy>(), 5);
            recipe.AddIngredient(ItemID.CrystalShard, 1);
            recipe.AddTile(TileID.Furnaces);
        }
    }

    public class Trellis : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/Trellis";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trellis");
            Tooltip.SetDefault("Places soil with a trellis on it");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<TrellisTile>();
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.AddIngredient(ItemType<Soil>(), 1);
            recipe.AddTile(TileID.WorkBenches);
        }
    }

    public class Planter : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/Planter";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hanging Planter");
            Tooltip.SetDefault("Used to grow hanging plants");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<PlanterTile>();
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ClayBlock, 5);
            recipe.AddIngredient(ItemID.Chain, 1);
            recipe.AddTile(TileID.WorkBenches);
        }
    }
}