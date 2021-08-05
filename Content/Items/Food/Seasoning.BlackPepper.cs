using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class BlackPepper : Ingredient
    {
        public BlackPepper() : base("Food buffs are 15% more effective", 300, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.White;
            if(StarlightRiver.Instance.HasLoaded && Main.rand.Next(10000) == 0)
                item.SetNameOverride("Grandma's ashes");
        }

        public override void BuffEffects(Player player, float multiplier)
        {
            player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.15f;
        }
    }
}