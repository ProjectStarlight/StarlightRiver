using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Food.Content.Side
{
    internal class Greens : Ingredient
    {
        public override string Texture => "StarlightRiver/Assets/Items/Food/Greens";

        public Greens() : base("+1 defense", 300, IngredientType.Side) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.statDefense += (int)(1 * multiplier);
        }
    }
}
