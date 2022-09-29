using StarlightRiver.Content.Tiles.Moonstone;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class MoonstoneOreItem : QuickTileItem
    {
        public MoonstoneOreItem() : base("Moonstone", "", "MoonstoneOre", ItemRarityID.Blue, AssetDirectory.MoonstoneItem) { }

        public override void SafeSetDefaults() => Item.value = Item.sellPrice(0, 0, 1, 50);
    }

    public class MoonstoneBarItem : QuickTileItem
    {
        public MoonstoneBarItem() : base("Moonstone Bar", "'Shimmering with Beautiful Light'", "MoonstoneBar", ItemRarityID.White, AssetDirectory.MoonstoneItem) { }  //TODO: Fix place type

        public override void SafeSetDefaults() => Item.value = Item.sellPrice(0, 0, 13, 50);

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<MoonstoneOreItem>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();

            //adds back neccisary vanilla recipies
            Recipe recipe2 = Recipe.Create(ItemID.DrillContainmentUnit);
            recipe2.AddIngredient(ItemID.LunarBar, 40);
            recipe2.AddIngredient(ItemID.ChlorophyteBar, 40);
            recipe2.AddIngredient(ItemID.ShroomiteBar, 40);
            recipe2.AddIngredient(ItemID.SpectreBar, 40);
            recipe2.AddIngredient(ItemID.HellstoneBar, 40);
            recipe2.AddIngredient(ItemType<MoonstoneBarItem>(), 40);
            recipe2.AddTile(TileID.MythrilAnvil);
            recipe2.Register();
        }
    }
}