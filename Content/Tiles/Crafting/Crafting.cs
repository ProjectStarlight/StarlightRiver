using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.GUI;
using StarlightRiver.Items.Herbology.Materials;

namespace StarlightRiver.Content.Tiles.Crafting
{
    //TODO: Split this up later.

    internal class Oven : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.CraftingTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => this.QuickSetFurniture(3, 2, DustID.Stone, SoundID.Dig, false, new Color(113, 113, 113), false, false, "Oven");

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => (r, g, b) = (0.4f, 0.2f, 0.05f);

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<OvenItem>());
    }

    internal class HerbStation : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.CraftingTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => this.QuickSetFurniture(3, 2, DustID.t_LivingWood, SoundID.Dig, false, new Color(151, 107, 75), false, false, "Herbologist's Bench");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<HerbStationItem>());
    }

    internal class CookStation : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.CraftingTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => this.QuickSetFurniture(6, 4, DustID.t_LivingWood, SoundID.Dig, false, new Color(151, 107, 75), false, false, "Cooking Station");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<CookStationItem>());

        public override bool NewRightClick(int i, int j)
        {
            var state = UILoader.GetUIState<CookingUI>();
            if (!state.Visible) { state.Visible = true; Main.PlaySound(SoundID.MenuOpen); }
            else { state.Visible = false; Main.PlaySound(SoundID.MenuClose); }
            return true;
        }
    }

    public class OvenItem : QuickTileItem
    {
        public OvenItem() : base("Oven", "Used to bake items", TileType<Oven>(), 0, AssetDirectory.CraftingTile) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.StoneBlock, 20);
            recipe.AddIngredient(ItemID.Gel, 5);
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class HerbStationItem : QuickTileItem
    {
        public HerbStationItem() : base("Herbologist's Bench", "Used to refine herbs", TileType<HerbStation>(), 0, AssetDirectory.CraftingTile) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(ItemType<Ivy>());
            recipe.AddIngredient(ItemID.Bottle, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class CookStationItem : QuickTileItem
    {
        public CookStationItem() : base("Prep Station", "Right click to prepare meals", TileType<CookStation>(), 0, AssetDirectory.CraftingTile) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(RecipeGroupID.IronBar, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}