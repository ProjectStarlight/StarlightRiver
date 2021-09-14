using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class TableSalt : Ingredient
    {
        public TableSalt() : base("Food buffs are 5% more effective", 2400, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            item.value = 400;
            item.rare = ItemRarityID.White;
        }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.05f;
        }
    }
}