using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using Terraria.ID;
using static Humanizer.In;

namespace StarlightRiver.Content.Items.Snow
{
	public class AquamarinePendant : SmartAccessory
	{
		private bool hasBarrier = false;

		public override string Texture => AssetDirectory.SnowItem + Name;

		public AquamarinePendant() : base("Aquamarine Pendant", "+15 Barrier \nLosing all of your barrier releases ice shards") { }

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			BarrierPlayer bPlayer = player.GetModPlayer<BarrierPlayer>();
			bPlayer.maxBarrier += 15;

			if (bPlayer.barrier > 0)
			{
				hasBarrier = true;
			}
			else if (hasBarrier)
			{
				hasBarrier = false;

				float rotOffset = Main.rand.NextFloat(6.28f);
				for (float i = rotOffset; i < 6.28f + rotOffset; i += (float)Math.PI * 0.3f)
				{
					Vector2 direction = i.ToRotationVector2();
					var proj = Projectile.NewProjectileDirect(player.GetSource_Accessory(Item), player.Center + direction * 10, direction * Main.rand.NextFloat(3, 5), ModContent.ProjectileType<BizarreIce>(), 15, 4, player.whoAmI);
					var mp = proj.ModProjectile as BizarreIce;
					mp.dontHit = default;
				}
			}

		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
		}
	}
}