using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class SquidBossSpawn : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + "SquidBossSpawn";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Suspicious Looking Offering");
            Tooltip.SetDefault("Drop in prismatic waters to summon the one the Squiddites worship");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 20;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddRecipeGroup("StarlightRiver:Fish", 5).
                AddIngredient(ItemID.CrimtaneOre, 4).
                AddTile(TileID.DemonAltar).
                Register();

            CreateRecipe().
                AddRecipeGroup("StarlightRiver:Fish", 5).
                AddIngredient(ItemID.DemoniteOre, 4).
                AddTile(TileID.DemonAltar).
                Register();

            CreateRecipe().
                AddRecipeGroup("StarlightRiver:Fish", 5).
                AddIngredient(ItemID.CrimtaneBar).
                AddTile(TileID.DemonAltar).
                Register();

            CreateRecipe().
                AddRecipeGroup("StarlightRiver:Fish", 5).
                AddIngredient(ItemID.DemoniteBar).
                AddTile(TileID.DemonAltar).
                Register();
        }
    }
}