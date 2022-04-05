using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class MoonstoneOre : QuickTileItem
    {
        public MoonstoneOre() : base("Moonstone", "", "MoonstoneOre", ItemRarityID.Blue, AssetDirectory.MoonstoneItem) { }

        public override void SafeSetDefaults() => Item.value = Item.sellPrice(0, 0, 1, 50);
    }

    public class MoonstoneBar : QuickTileItem
    {
        public MoonstoneBar() : base("Moonstone Bar", "'Shimmering with Beautiful Light'", "MoonstoneBar", ItemRarityID.White, AssetDirectory.MoonstoneItem) { }  //TODO: Fix place type

        public override void SafeSetDefaults() => Item.value = Item.sellPrice(0, 0, 13, 50);

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<MoonstoneOre>(), 3);
            recipe.AddTile(TileID.Furnaces);

            //adds back neccisary vanilla recipies
            Recipe recipe2 = Mod.CreateRecipe(ItemID.DrillContainmentUnit);
            recipe2.AddIngredient(ItemID.LunarBar, 40);
            recipe2.AddIngredient(ItemID.ChlorophyteBar, 40);
            recipe2.AddIngredient(ItemID.ShroomiteBar, 40);
            recipe2.AddIngredient(ItemID.SpectreBar, 40);
            recipe2.AddIngredient(ItemID.HellstoneBar, 40);
            recipe2.AddIngredient(ItemType<MoonstoneBar>(), 40);
            recipe2.AddTile(TileID.MythrilAnvil);
        }
    }
}