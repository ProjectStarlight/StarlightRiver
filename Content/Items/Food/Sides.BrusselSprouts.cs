using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class BrusselSprouts : Ingredient
    {
        public BrusselSprouts() : base("+1 defense", 240, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.statDefense += (int)(1 * multiplier);
        }
    }
}
