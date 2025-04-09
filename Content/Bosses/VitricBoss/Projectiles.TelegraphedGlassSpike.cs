using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	public class TelegraphedGlassSpike : ModProjectile
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

			Color color = Helpers.CommonVisualEffects.HeatedToCoolColor(MathHelper.Min(200 - Projectile.timeLeft, 120));
			color.A = 0;

			if (Projectile.timeLeft < 165)
			{
				for (int k = 0; k <= 1; k++)
				{
					Vector2 pos = Projectile.Center + Vector2.Normalize(Projectile.velocity).RotatedBy(1.57f) * (k == 0 ? 6f : -6f);
					var d = Dust.NewDustPerfect(pos, DustType<Dusts.PixelatedImpactLineDust>(), (Projectile.velocity * Main.rand.NextFloat(-0.5f, -0.02f)).RotatedBy(k == 0 ? -0.4f : 0.4f), 0, color, 0.1f);
					d.customData = 0.85f;
				}
			}

			Projectile.rotation = savedVelocity.ToRotation() + 3.14f / 4;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.Bleeding, 300);
		}

		public override void OnKill(int timeLeft)
		{
			Color color = CommonVisualEffects.HeatedToCoolColor(MathHelper.Min(200 - Projectile.timeLeft, 120));
			color.A = 0;

			for (int k = 0; k <= 10; k++)
			{
				Dust.NewDust(Projectile.position, 22, 22, DustType<Dusts.GlassGravity>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(4), 0, color, Main.rand.NextFloat(0.2f, 0.4f));
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			if (Projectile.timeLeft > 180)
				return false;

			Color color = CommonVisualEffects.HeatedToCoolColor(MathHelper.Min(200 - Projectile.timeLeft, 120));

			spriteBatch.Draw(Assets.Bosses.VitricBoss.GlassSpike.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 22, 22), lightColor, Projectile.rotation, Vector2.One * 11, Projectile.scale, 0, 0);
			spriteBatch.Draw(Assets.Bosses.VitricBoss.GlassSpike.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 22, 22, 22), color, Projectile.rotation, Vector2.One * 11, Projectile.scale, 0, 0);

			return false;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Bosses.VitricBoss.GlassSpikeGlow.Value;
			float alpha = Projectile.timeLeft > 160 ? 1 - (Projectile.timeLeft - 160) / 20f : 1;
			Color color = CommonVisualEffects.HeatedToCoolColor(MathHelper.Min(200 - Projectile.timeLeft, 120)) * alpha;
			color.A = 0;

			Main.spriteBatch.Draw(tex, Projectile.Center + Vector2.Normalize(Projectile.velocity) * -40 - Main.screenPosition, tex.Frame(), color * (Projectile.timeLeft / 140f), Projectile.rotation + 3.14f, tex.Size() / 2, 1.8f, 0, 0);

			if (Projectile.timeLeft > 180)
			{
				Texture2D tex2 = Assets.Masks.LineAlpha.Value;
				float alpha2 = (float)Math.Sin((Projectile.timeLeft - 180) / 60f * 3.14f);
				Color color2 = new Color(255, 180, 80, 0) * alpha2;
				var source = new Rectangle(0, tex2.Height / 2, tex2.Width, tex2.Height / 2);
				Main.spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, source, color2, Projectile.rotation - 3.14f / 4 - 3.14f / 2, new Vector2(tex2.Width / 2, 0), 6, 0, 0);
			}
		}
	}
}