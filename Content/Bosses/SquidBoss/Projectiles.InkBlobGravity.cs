using System;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class InkBlobGravity : InkBlob
	{
		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 200;
			Projectile.hostile = true;
			Projectile.damage = 25;
		}

		public override void AI()
		{
			Projectile.scale -= 1 / 400f;

			Projectile.ai[1] += 0.1f;
			Projectile.rotation += Main.rand.NextFloat(0.2f);
			Projectile.scale = 0.5f;

			Projectile.velocity.Y += 0.2f;

			float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
			Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			if (Main.masterMode)
				color = new Color(1, 0.25f + sin * 0.25f, 0f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

			if (Main.rand.NextBool(4))
			{
				var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, color, 0.5f);
				d.customData = Main.rand.NextFloat(1, 2);
			}

			ManageCaches();
			ManageTrail();
		}
	}
}
