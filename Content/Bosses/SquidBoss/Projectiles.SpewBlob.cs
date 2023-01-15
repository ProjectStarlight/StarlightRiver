using System;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class SpewBlob : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Shard");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 50;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 42;
			Projectile.height = 42;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 300;
			Projectile.hostile = true;
			Projectile.damage = 20;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			if (Projectile.ai[1] == 0)
			{
				Projectile.width = (int)(42 * Projectile.scale);
				Projectile.height = (int)(42 * Projectile.scale);
			}

			Projectile.velocity.Y -= 0.025f;
			Projectile.velocity.X *= 0.9f;

			Projectile.ai[1] += 0.05f;
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

			float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
			var color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.1f);

			if (Main.rand.NextBool(10))
			{
				var d = Dust.NewDustPerfect(Projectile.Center, 264, -Projectile.velocity.RotatedByRandom(0.25f) * 0.75f, 0, color, 1);
				d.noGravity = true;
				d.rotation = Main.rand.NextFloat(6.28f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D star = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "OrbitalStrike").Value;

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				float sin = 1 + (float)Math.Sin(Projectile.ai[1] + k * 0.1f);
				float cos = 1 + (float)Math.Cos(Projectile.ai[1] + k * 0.1f);
				Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (1 - k / (float)Projectile.oldPos.Length);

				if (Main.masterMode)
					color = new Color(1, 0.5f + sin * 0.25f, 0.25f) * (1 - k / (float)Projectile.oldPos.Length);

				Main.spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, Projectile.oldRot[k], tex.Size() / 2, Projectile.scale * (0.85f - k / (float)Projectile.oldPos.Length * 0.85f), default, default);

				if (k == 0)
				{
					Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color, Projectile.ai[1], star.Size() / 2, Projectile.scale * 0.65f, default, default);
					Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color * 0.75f, Projectile.ai[1] * -0.2f, star.Size() / 2, Projectile.scale * 0.85f, default, default);
					Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color * 0.6f, Projectile.ai[1] * -0.6f, star.Size() / 2, Projectile.scale * 1.15f, default, default);
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int n = 0; n < 20; n++)
			{
				float sin = 1 + (float)Math.Sin(Projectile.ai[1] + n * 0.1f);
				float cos = 1 + (float)Math.Cos(Projectile.ai[1] + n * 0.1f);
				var color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

				var d = Dust.NewDustPerfect(Projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color, 2.2f);
				d.noGravity = true;
				d.rotation = Main.rand.NextFloat(6.28f);
			}
		}
	}
}
