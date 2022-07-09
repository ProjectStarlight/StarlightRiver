using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class RocketFuel : Ingredient
    {
        public RocketFuel() : base("+500% effectiveness", -3840, IngredientType.Main) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier += 5f;
        }
    }
}