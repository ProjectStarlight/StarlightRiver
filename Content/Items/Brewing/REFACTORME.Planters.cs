using Terraria.ID;
using Terraria.ModLoader;

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
            Item.createTile = Mod.TileType("Soil");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.MudBlock, 10);
            recipe.AddIngredient(Mod.ItemType("Ivy"), 5);
            recipe.AddIngredient(ItemID.CrystalShard, 1);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this, 10);
            recipe.AddRecipe();
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
            Item.createTile = Mod.TileType("Trellis");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.AddIngredient(Mod.ItemType("Soil"), 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
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
            Item.createTile = Mod.TileType("Planter");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.ClayBlock, 5);
            recipe.AddIngredient(ItemID.Chain, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }
}