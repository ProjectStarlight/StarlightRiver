using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class StarlightWater : Ingredient
    {
        public StarlightWater() : base("Regenerate 4 mana per second constantly", 360, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

        public override void BuffEffects(Player Player, float multiplier)
        {
            int interval = (int)(60 / (4 * multiplier));

            if (Player.GetModPlayer<StarlightPlayer>().Timer % interval == 0)
                Player.statMana++;
        }
    }
}