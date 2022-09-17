using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class CoffeeBeans : Ingredient
    {
        public CoffeeBeans() : base("+10% critical strike damage\n+20% duration", 180, IngredientType.Main, 1.2f) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += OnHit;
			StarlightPlayer.ModifyHitNPCWithProjEvent += OnHitProj;
		}

		private void OnHitProj(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            if (crit)
				damage = (int)(damage * (1 + (0.1f * player.GetModPlayer<FoodBuffHandler>().Multiplier)));
		}

		private void OnHit(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (crit)
				damage = (int)(damage * (1 + (0.1f * player.GetModPlayer<FoodBuffHandler>().Multiplier)));
		}
	}
}