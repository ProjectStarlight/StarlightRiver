using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric.IgnitionGauntlets
{
	public class IgnitionPunchPhantom : ModProjectile
	{

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public Vector2 directionVector = Vector2.Zero;
		private Player owner => Main.player[Projectile.owner];

		private bool front => Projectile.ai[0] == 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 13;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Vector2 direction = Projectile.DirectionTo(Main.MouseWorld);
			Projectile.Center = owner.Center + direction * 20;

			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			float extend = (float)Math.Sin(Projectile.timeLeft / 2f);
			if (extend < 0.25f)
				stretch = Player.CompositeArmStretchAmount.None;
			else if (extend < 0.5f)
				stretch = Player.CompositeArmStretchAmount.Quarter;
			else if (extend < 0.8f)
				stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
			else
				stretch = Player.CompositeArmStretchAmount.Full;
			if (front)
				owner.SetCompositeArmFront(true, stretch, directionVector.ToRotation() - 1.57f);
			else
				owner.SetCompositeArmBack(true, stretch, directionVector.ToRotation() - 1.57f);

		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			//Projectile.penetrate++;
			//Projectile.friendly = false;
			//owner.velocity = -owner.DirectionTo(Projectile.Center) * 6;
		}
	}
}