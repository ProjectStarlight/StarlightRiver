using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	public class TelegraphedGlassSpike : ModProjectile, IDrawAdditive
	{
		Vector2 savedVelocity;

		public override string Texture => AssetDirectory.VitricBoss + "GlassSpike";

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.damage = 5;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glass Spike");
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 240)
			{
				savedVelocity = Projectile.velocity;
				Projectile.velocity *= 0;
			}

			if (Projectile.timeLeft > 150)
				Projectile.velocity = Vector2.SmoothStep(Vector2.Zero, savedVelocity, (30 - (Projectile.timeLeft - 150)) / 30f);

			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120));

			if (Projectile.timeLeft < 165)
			{
				for (int k = 0; k <= 1; k++)
				{
					Vector2 pos = Projectile.Center + Vector2.Normalize(Projectile.velocity).RotatedBy(1.57f) * (k == 0 ? 10f : -10f);
					var d = Dust.NewDustPerfect(pos, DustType<Dusts.GlowLine>(), (Projectile.velocity * Main.rand.NextFloat(-0.25f, -0.1f)).RotatedBy(k == 0 ? -0.4f : 0.4f), 0, color, 1f);
					d.customData = 0.85f;
				}
			}

			Projectile.rotation = savedVelocity.ToRotation() + 3.14f / 4;
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.AddBuff(BuffID.Bleeding, 300);
		}

		public override void Kill(int timeLeft)
		{
			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120));

			for (int k = 0; k <= 10; k++)
			{
				Dust.NewDust(Projectile.position, 22, 22, DustType<Dusts.GlassGravity>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
				Dust.NewDust(Projectile.position, 22, 22, DustType<Dusts.Glow>(), 0, 0, 0, color, 0.3f);
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			if (Projectile.timeLeft > 180)
				return false;

			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120));

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 22, 22), lightColor, Projectile.rotation, Vector2.One * 11, Projectile.scale, 0, 0);
			spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 22, 22, 22), color, Projectile.rotation, Vector2.One * 11, Projectile.scale, 0, 0);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			float alpha = Projectile.timeLeft > 160 ? 1 - (Projectile.timeLeft - 160) / 20f : 1;
			Color color = Helpers.Helper.MoltenVitricGlow(MathHelper.Min(200 - Projectile.timeLeft, 120)) * alpha;

			spriteBatch.Draw(tex, Projectile.Center + Vector2.Normalize(Projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
				color * (Projectile.timeLeft / 140f), Projectile.rotation + 3.14f, tex.Size() / 2, 1.8f, 0, 0);

			if (Projectile.timeLeft > 180)
			{
				Texture2D tex2 = Request<Texture2D>(AssetDirectory.VitricBoss + "RoarLine").Value;
				float alpha2 = (float)Math.Sin((Projectile.timeLeft - 180) / 60f * 3.14f);
				Color color2 = new Color(255, 180, 80) * alpha2;
				var source = new Rectangle(0, tex2.Height / 2, tex2.Width, tex2.Height / 2);
				spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, source, color2, Projectile.rotation - 3.14f / 4 - 3.14f / 2, new Vector2(tex2.Width / 2, 0), 6, 0, 0);
			}
		}
	}
}