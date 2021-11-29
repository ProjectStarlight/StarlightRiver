using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria;

namespace StarlightRiver.Content.Tiles.AstralMeteor
{
	public class AluminumBar : ModTile
    {
        public override bool Autoload(ref string name, ref string texture) {
            texture = AssetDirectory.AluminumTile + name;
            return base.Autoload(ref name, ref texture); }

        public override void SetDefaults() =>
            this.QuickSetBar(ItemType<AluminumBarItem>(), DustType<Dusts.Electric>(), new Color(156, 172, 177));
    }

    public class AluminumBarItem : QuickTileItem
    {
        public AluminumBarItem() : base("Astral Aluminum Bar", "'Shimmering with Beautiful Light'", "AluminumBar", ItemRarityID.White, AssetDirectory.AluminumTile) { }  //TODO: Fix place type

        public override void SafeSetDefaults() => item.value = Item.sellPrice(0, 0, 14, 0);

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<AluminumOreItem>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();

            //adds back neccisary vanilla recipies
            ModRecipe recipe2 = new ModRecipe(mod);
            recipe2.AddIngredient(ItemID.LunarBar, 40);
            recipe2.AddIngredient(ItemID.ChlorophyteBar, 40);
            recipe2.AddIngredient(ItemID.ShroomiteBar, 40);
            recipe2.AddIngredient(ItemID.SpectreBar, 40);
            recipe2.AddIngredient(ItemID.HellstoneBar, 40);
            recipe2.AddIngredient(ItemType<AluminumBarItem>(), 40);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.SetResult(ItemID.DrillContainmentUnit);
            recipe2.AddRecipe();
        }
    }
}