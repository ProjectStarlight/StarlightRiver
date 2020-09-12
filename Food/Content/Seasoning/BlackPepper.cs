using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Food.Content.Seasoning
{
    internal class BlackPepper : Ingredient
    {
        public BlackPepper() : base("Food buffs are 15% more effective", 300, IngredientType.Seasoning) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.15f;
        }
    }
}