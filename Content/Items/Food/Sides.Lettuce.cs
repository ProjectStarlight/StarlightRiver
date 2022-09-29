using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class Lettuce : Ingredient
    {
        public Lettuce() : base("+40 maximum barrier", 60, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<BarrierPlayer>().MaxBarrier += 40;
        }
    }
}