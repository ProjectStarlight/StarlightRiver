using System;

namespace StarlightRiver.Compat.BossChecklist
{
	class AuroraclePortrait
	{
		private readonly static ParticleSystem auroracleSystem = new("StarlightRiver/Assets/Dusts/Aurora", n =>
		{
			n.Velocity *= 0.995f;
			n.Position += n.Velocity;
			n.Scale *= 0.99f;
			n.Rotation += 0.05f;

			float r = 0.25f * (1f + (float)Math.Sin(n.Timer / 30f + n.Position.X * 0.05f * 0.2));
			float g = 0.35f * (1f + (float)Math.Cos(n.Timer / 30f + n.Position.X * 0.05f));
			float b = 0.45f;

			float a = 1;

			if (n.Timer > 270)
				a = 1 - (n.Timer - 270) / 30f;

			n.Color = Color.Lerp(new Color(r, g, b), Color.White, 0.25f) * (n.Timer / 300f) * a;

			n.Timer--;
		});

		public static void DrawAuroraclePortrait(SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			if (Main.rand.NextBool(3))
				auroracleSystem.AddParticle(new Particle(rect.Center() + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(120), Vector2.UnitY * -Main.rand.NextFloat(), Main.rand.NextFloat(6.28f), Main.rand.NextFloat(0.25f, 0.45f), new Color(200, 200, 0), 300, Vector2.Zero, new Rectangle(0, 0, 100, 100)));

			//float sin = 0.8f + (float)Math.Sin(Main.GameUpdateCount / 100f) * 0.1f;

			Texture2D body = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/SquidBoss").Value;
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/SquidBossGlow").Value;
			Texture2D blur = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/SquidBossGlowBlur").Value;
			Texture2D specular = ModContent.Request<Texture2D>("StarlightRiver/Assets/BossChecklist/SquidBossSpecular").Value;
			Texture2D circleGlow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

			spriteBatch.Draw(circleGlow, rect, null, Color.Black * 0.6f, 0, Vector2.UnitY * 2, 0, 0);

			uint GlobalTimer = Main.GameUpdateCount;

			float sin = 1 + (float)Math.Sin(GlobalTimer / 10f * 0.5f);
			float sin2 = 1 + (float)Math.Sin(GlobalTimer / 10f * 0.5f + 1.5f);
			float cos = 1 + (float)Math.Cos(GlobalTimer / 10f * 0.5f);

			var glowColor = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f);

			spriteBatch.Draw(glow, rect.Center(), null, glowColor, 0, glow.Size() / 2, 1, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.Draw(blur, rect.Center(), null, glowColor * 0.55f, 0, blur.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(blur, rect.Center(), null, glowColor * 0.3f, 0, blur.Size() / 2, 1.05f, 0, 0);

			auroracleSystem.DrawParticles(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.Draw(body, rect.Center(), null, color, 0, body.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(specular, rect.Center(), null, Color.White, 0, specular.Size() / 2, 1, 0, 0);

			auroracleSystem.SetTexture(ModContent.Request<Texture2D>("StarlightRiver/Assets/Dusts/Aurora").Value);
		}
	}
}