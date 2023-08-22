﻿using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Beach
{
	internal class SeaglassLens : SmartAccessory
	{
		public override string Texture => AssetDirectory.Assets + "Items/Beach/" + Name;

		public SeaglassLens() : base("Seaglass Lens", "Your minions and sentries occasionally deal extra damage\nThis extra damage counts as a critical strike") { }

		public override void Load()
		{
			StarlightProjectile.OnHitNPCEvent += BonusDamage;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}

		private void BonusDamage(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[projectile.owner];

			if (owner == Main.LocalPlayer && Equipped(owner) && projectile.DamageType == DamageClass.Summon && Main.rand.NextBool(10))
				target.SimpleStrikeNPC(damageDone / 2, hit.HitDirection, true);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SeaglassRing>());
			recipe.AddIngredient(ItemID.Glass, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}