using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class WhippedCream : Ingredient
    {
        public WhippedCream() : base("10% increased movement speed\n-20% duration", 300, IngredientType.Seasoning, 0.8f) { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.White;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.accRunSpeed += 0.5f;
        }
    }
}