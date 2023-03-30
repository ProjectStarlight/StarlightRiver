using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class SkySprinkles : Ingredient
	{
		public SkySprinkles() : base("Regen mana on hit\nWip", 180, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += OnHit;
			StarlightPlayer.ModifyHitNPCWithProjEvent += OnHitProj;
		}

		private void OnHitProj(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			int amount = (int)(1 * player.GetModPlayer<FoodBuffHandler>().Multiplier);
			player.ManaEffect(amount);
			player.statMana += amount;
		}

		private void OnHit(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			int amount = (int)(1 * player.GetModPlayer<FoodBuffHandler>().Multiplier);
			player.ManaEffect(amount);
			player.statMana += amount;
		}
	}
}