using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class DoorBomb : InteractiveProjectile
	{
		public override string Texture => AssetDirectory.SquidBoss + "SpewBlob";

		public override void GoodEffects()
		{
			StarlightWorld.Flag(WorldFlags.SquidBossOpen);
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 176;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 175)
				ValidPoints.Add(new Point16((int)Projectile.Center.X / 16 + 11, (int)Projectile.Center.Y / 16));

			Projectile.ai[1] += 0.1f;
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

			float sin = 1 + (float)Math.Sin(Projectile.ai[1]);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1]);
			var color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.6f);

			var d = Dust.NewDustPerfect(Projectile.Center, 264, -Projectile.velocity * 0.5f, 0, color, 1.4f);
			d.noGravity = true;
			d.rotation = Main.rand.NextFloat(6.28f);
		}

		public override void SafeKill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
			{
				var d = Dust.NewDustPerfect(Projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(), 0, Color.White, 1.4f);
				d.noGravity = true;
				d.rotation = Main.rand.NextFloat(6.28f);
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
		}
	}
}