using Terraria;

namespace StarlightRiver.Food
{
    internal class RoastRabbit : Ingredient
    {
        public RoastRabbit() : base("+5% melee damage", 600, IngredientType.Main) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.meleeDamageMult += 0.05f * multiplier;
        }
    }

    internal class Greens : Ingredient
    {
        public Greens() : base("+1 defense", 300, IngredientType.Side) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.statDefense += (int)(1 * multiplier);
        }
    }
}