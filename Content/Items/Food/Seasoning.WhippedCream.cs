using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class WhippedCream : Ingredient
    {
        public WhippedCream() : base("Food buffs are 20% less effective\n 10% increased movement speed", 300, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.White;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.2f;
            Player.accRunSpeed += 0.5f;
        }
    }
}