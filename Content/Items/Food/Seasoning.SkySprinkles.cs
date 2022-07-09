using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class SkySprinkles : Ingredient
    {
        public SkySprinkles() : base("Regen mana on hit\nWip", 180, IngredientType.Seasoning) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

        public override void BuffEffects(Player Player, float multiplier)
        {

        }
    }
}