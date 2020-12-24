using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Food
{
    internal class Greens : Ingredient
    {
        public Greens() : base("+1 defense", 300, IngredientType.Side) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.statDefense += (int)(1 * multiplier);
        }
    }
}
