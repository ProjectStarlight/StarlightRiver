using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class Entrails : Ingredient
    {
        public Entrails() : base("+20% damage and -20% max health\n-20% duration", 360, IngredientType.Main, 0.8f) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetDamage(DamageClass.Generic) += 0.2f * multiplier;
            Player.statLifeMax2 -= (int)(Player.statLifeMax2 * (0.2f * multiplier));
        }
    }
}