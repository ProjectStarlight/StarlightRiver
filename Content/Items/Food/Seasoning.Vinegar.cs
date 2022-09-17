using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Vinegar : Ingredient
    {
        public Vinegar() : base("Food buffs are 5% less effective\n+10% duration", 120, IngredientType.Seasoning, 1.1f) { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.White;
            Item.value = 1000;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.4f;
        }
    }
}