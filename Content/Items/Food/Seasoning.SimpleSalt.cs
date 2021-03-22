using Terraria;

using StarlightRiver.Core;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
    internal class SimpleSalt : Ingredient
    {
        public SimpleSalt() : base("Food buffs are 5% more effective", 2400, IngredientType.Seasoning) { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.White;

        public override void BuffEffects(Player player, float multiplier)
        {
            player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.05f;
        }
    }
}