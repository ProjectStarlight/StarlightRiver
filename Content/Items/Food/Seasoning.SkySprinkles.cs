using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class SkySprinkles : Ingredient
	{
		public SkySprinkles() : base("Regen mana on hit\nWip", 180, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 3);
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += OnHit;
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitProj;
		}

		private void OnHitProj(Player player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Active(player))
			{
				int amount = (int)(1 * player.GetModPlayer<FoodBuffHandler>().Multiplier);
				player.ManaEffect(amount);
				player.statMana += amount;
			}
		}

		private void OnHit(Player player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Active(player))
			{
				int amount = (int)(1 * player.GetModPlayer<FoodBuffHandler>().Multiplier);
				player.ManaEffect(amount);
				player.statMana += amount;
			}
		}
	}
}