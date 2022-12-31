using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric.IgnitionGauntlets
{
	public class IgnitionGauntletCone : ModProjectile
	{
		public Vector2 directionVector = Vector2.Zero;

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 5;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for (float i = -0.7f; i < 0.7f; i += 0.05f)
			{
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (Projectile.rotation + i).ToRotationVector2() * 220 * Projectile.ai[0]))
					return true;
			}

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}
	}
}