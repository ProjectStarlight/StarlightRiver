using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Aluminum
{
    public class AluminumOre : QuickTileItem
    {
        public AluminumOre() : base("Astral Aluminum", "", TileType<Tiles.OreAluminum>(), ItemRarityID.White) { }

        public override void SafeSetDefaults() => item.value = Item.sellPrice(0, 0, 2, 0);
    }

    public class AluminumBar : QuickTileItem
    {
        public AluminumBar() : base("Astral Aluminum Bar", "'Shimmering with Beautiful Light'", TileType<Tiles.AluminumBar>(), ItemRarityID.Blue) { }

        public override void SafeSetDefaults() => item.value = Item.sellPrice(0, 0, 14, 0);

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<AluminumOre>(), 3);
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
            recipe2.AddIngredient(ItemType<AluminumBar>(), 40);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.SetResult(ItemID.DrillContainmentUnit);
            recipe2.AddRecipe();
        }
    }
}