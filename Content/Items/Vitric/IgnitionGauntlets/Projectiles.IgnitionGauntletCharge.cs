using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric.IgnitionGauntlets
{
	public class IgnitionGauntletCharge : ModProjectile
	{
		int charge = 0;

		private Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.VitricItem + "IgnitionGauntletLaunch_Star";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			Projectile.hide = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overPlayers.Add(index);
			base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			IgnitionPlayer modPlayer = Owner.GetModPlayer<IgnitionPlayer>();
			Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 handOffset = new Vector2(8 * Owner.direction, 0).RotatedBy(Owner.fullRotation);
			Main.spriteBatch.Draw(starTex, Owner.MountedCenter + handOffset - Main.screenPosition, null, new Color(255, 255, 255, 0) * (charge / (float)modPlayer.charge), Main.GameUpdateCount * 0.085f, starTex.Size() / 2, 0.5f + 0.07f * (float)Math.Sin(Main.GameUpdateCount * 0.285f), SpriteEffects.None, 0f);
			return false;
		}

		public override void AI()
		{
			IgnitionPlayer modPlayer = Owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = Owner.Center;

			if (Main.mouseRight)
			{
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f * Owner.direction);

				if (modPlayer.charge > charge)
					charge += 3;

				if (modPlayer.charge - 50 > charge)
				{
					var dust = Dust.NewDustPerfect(Owner.Center + new Vector2(Main.rand.Next(-25, 25), 25), ModContent.DustType<IgnitionChargeDust>(), default, default, Color.OrangeRed);
					dust.customData = Owner.whoAmI;
				}

				modPlayer.potentialCharge = charge;
			}
			else
			{
				modPlayer.potentialCharge = 0;

				if (!Owner.GetModPlayer<IgnitionPlayer>().launching)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletLaunch>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

				modPlayer.charge -= charge;
				modPlayer.loadedCharge = charge;
				Owner.GetModPlayer<IgnitionPlayer>().launching = true;
				Projectile.active = false;
			}
		}
	}
}