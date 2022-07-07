using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class CoffeeBeans : Ingredient
    {
        public CoffeeBeans() : base("+12% critical strike chance", 2880, IngredientType.Main) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetCritChance(DamageClass.Generic) += 0.12f * multiplier;//this needs to be crit damage
        }
    }
}