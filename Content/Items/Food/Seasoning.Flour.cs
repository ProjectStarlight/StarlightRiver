using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Flour : Ingredient
    {
        public Flour() : base("Food buffs are 3% less effective", 600, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            Item.value = 100;
            Item.rare = ItemRarityID.White;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.03f;
        }
    }
}