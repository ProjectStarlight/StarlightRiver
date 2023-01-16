using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class Butter : Ingredient
    {
        public Butter() : base("+20 maximum life", 300, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.statLifeMax2 += 20;
        }
    }
}