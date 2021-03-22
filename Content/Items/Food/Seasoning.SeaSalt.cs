using Terraria;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Cooking;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
    internal class SeaSalt : Ingredient
    {
        public SeaSalt() : base("Food buffs are 10% more effective\nYou can breathe under water\nTest effect\nTest effect 2\nTest effect with a really long description so it will have to go on multiple lines", 1200, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            item.createTile = TileType<Tiles.Cooking.SeaSalt>();
            item.rare = ItemRarityID.Blue;
        }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.1f;
            player.gills = true;
        }
    }
}