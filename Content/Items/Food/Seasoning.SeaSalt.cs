using Terraria;
using Terraria.ID;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Food
{
	internal class SeaSalt : Ingredient
    {
        public SeaSalt() : base("Food buffs are 10% more effective\nYou can breathe under water", 1200, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;

            Item.createTile = TileType<Tiles.Food.SeaSalt>();
            Item.value = Item.sellPrice(0, 0, 8, 0);
            Item.rare = ItemRarityID.Blue;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.1f;
            Player.gills = true;
        }
    }
}