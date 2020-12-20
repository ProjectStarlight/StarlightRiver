using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

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
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createTile = mod.TileType("Soil");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MudBlock, 10);
            recipe.AddIngredient(mod.ItemType("Ivy"), 5);
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
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createTile = mod.TileType("Trellis");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.AddIngredient(mod.ItemType("Soil"), 1);
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
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createTile = mod.TileType("Planter");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ClayBlock, 5);
            recipe.AddIngredient(ItemID.Chain, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }

    public class Greenhouse : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/Greenhouse";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Greenhouse Glass");
            Tooltip.SetDefault("Fancy!");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createWall = mod.WallType("GreenhouseWall");
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Glass, 10);
            recipe.AddIngredient(ItemID.Wood, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 10);
            recipe.AddRecipe();
        }
    }
}