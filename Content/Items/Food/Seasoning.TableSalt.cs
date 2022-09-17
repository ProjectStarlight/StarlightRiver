using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Food
{
	internal class TableSalt : Ingredient
    {
        public TableSalt() : base("Food buffs are 5% more effective", 900, IngredientType.Seasoning) { }

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

            Item.createTile = TileType<Tiles.Food.Salt>();
            Item.value = Item.sellPrice(0, 0, 4, 0);
            Item.rare = ItemRarityID.White;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.05f;
        }
    }
}