using ReLogic.Content;
using StarlightRiver.Content.Backgrounds;
using System;

namespace StarlightRiver.Content.Menus
{
	internal class DefaultStarlightMenu : ModMenu
	{
		static int timer = 0;

		public static ParticleSystem stars;

		public static float starOpacity = 1;

		public override string DisplayName => "Starlight";
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/StarBird");

		public override void Load()
		{
			stars = new("StarlightRiver/Assets/Misc/StarParticle", UpdateStars, ParticleSystem.AnchorOptions.Screen);
		}

		public static void DrawStars(SpriteBatch sb)
		{
			Texture2D texB = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			sb.Draw(texB, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			sb.End();
			sb.Begin(default, default, SamplerState.LinearWrap, default, RasterizerState.CullNone, default, Main.UIScaleMatrix);

			Texture2D tex = Assets.Noise.SwirlyNoiseLooping.Value;
			Texture2D tex2 = Assets.Noise.PerlinNoise.Value;
			Texture2D tex3 = Assets.Noise.MiscNoise3.Value;
			var color = new Color(100, 230, 220)
			{
				A = 0
			};

			var target = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

			color.G = 190;
			color.B = 255;
			Vector2 offset = Vector2.One * timer * 0.2f + Vector2.One.RotatedBy(timer * 0.01f) * 20;
			var source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			color.R = 150;
			color.G = 10;
			color.B = 255;
			color.A = 0;
			offset = Vector2.One * timer * 0.4f + Vector2.One.RotatedBy(timer * 0.005f) * 36;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex2, target, source, color * 0.05f);

			color.R = 10;
			color.G = 255;
			color.B = 255;
			color.A = 0;
			offset = Vector2.One * timer * 0.6f + Vector2.One.RotatedBy(timer * 0.021f) * 15;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex3, target, source, color * 0.05f);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.UIScaleMatrix);

			stars.DrawParticles(sb);

			sb.End();
			sb.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);
		}

		private static void UpdateStars(Particle particle)
		{
			if (particle.Type == 0) // River
			{
				particle.Timer--;
				particle.Position += particle.Velocity;

				particle.Position.Y = Main.screenHeight / 2f + (float)Math.Sin(particle.Position.X / Main.screenWidth * 6.28f) * Main.screenHeight / 8f + particle.StoredPosition.Y;

				particle.Alpha = starOpacity;

				if (particle.Timer < 30)
					particle.Alpha = particle.Timer / 30f * starOpacity;

				particle.Rotation += 0.2f * particle.Scale;
			}
			else if (particle.Type == 1) // Intro
			{
				particle.Timer--;
				particle.Position += particle.Velocity;

				particle.Scale = (1f - particle.Timer / 60f) * 2f;
			}
			else if (particle.Type == 2) // Background
			{
				particle.Timer--;
				particle.StoredPosition += particle.Velocity;

				particle.Rotation += 0.02f;

				if (particle.Timer > 1570)
					particle.Alpha = (1600 - particle.Timer) / 30f * starOpacity;

				if (particle.Timer < 30)
					particle.Alpha = particle.Timer / 30f * starOpacity;

				particle.Position = particle.StoredPosition + (particle.StoredPosition - (Main.screenPosition + Main.ScreenSize.ToVector2() / 2f)) * (particle.Scale * 6f - 0.35f * 3f) - Main.screenPosition;
			}
		}

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			logoScale = 1.0f;
			Main.dayTime = false;

			timer++;

			// star particles
			var starColor = new Color(150, Main.rand.Next(150, 255), 255)
			{
				A = 0
			};

			if (Main.rand.NextBool(2))
			{
				var pos = new Vector2(Main.rand.Next(0, (int)(Main.screenWidth * 2.5f)), Main.rand.Next(-Main.screenHeight / 2, (int)(Main.screenHeight * 1.5f)));
				stars.AddParticle(new Particle(pos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.25f), 0, Main.rand.NextFloat(0.35f), starColor * 0.8f, 1600, pos + Main.screenPosition, new Rectangle(0, 120, 120, 120), 1, 2));
			}

			float prog = Main.rand.NextFloat();
			starColor.G = (byte)(150 + prog * 105);

			bool star = Main.rand.NextBool(18);
			stars.AddParticle(new Particle(new Vector2(0, Main.screenHeight * 0.2f + prog * 0f), new Vector2(3f + prog * 4f, 1), 0, star ? Main.rand.NextFloat(0.2f, 0.3f) : Main.rand.NextFloat(0.05f, 0.1f), starColor * (star ? 1.2f : 1f), 600, Vector2.One * (prog * 110f), new Rectangle(0, star ? 120 : 0, 120, 120), 1));

			DrawStars(spriteBatch);

			// Rings
			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			Vector2 pos2 = Main.ScreenSize.ToVector2() / 2f;
			Color color = new Color(40, 50, 65);

			Texture2D smallRing = Assets.NPCs.BossRush.ArmillaryRing1.Value;
			Texture2D mediumRing = Assets.NPCs.BossRush.ArmillaryRing2.Value;
			Texture2D largeRing = Assets.NPCs.BossRush.ArmillaryRing3.Value;

			spriteBatch.Draw(smallRing, pos2, null, color, -timer * 0.005f, smallRing.Size() * 0.5f, 2, SpriteEffects.None, 0);
			spriteBatch.Draw(mediumRing, pos2, null, color, timer * 0.005f, mediumRing.Size() * 0.5f, 2, SpriteEffects.None, 0);
			spriteBatch.Draw(largeRing, pos2, null, color, -timer * 0.005f, largeRing.Size() * 0.5f, 2, SpriteEffects.None, 0);

			Texture2D smallRingRunes = Assets.NPCs.BossRush.ArmillaryRingRunes1.Value;
			Texture2D mediumRingRunes = Assets.NPCs.BossRush.ArmillaryRingRunes2.Value;
			Texture2D largeRingRunes = Assets.NPCs.BossRush.ArmillaryRingRunes3.Value;

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			spriteBatch.Draw(smallRingRunes, pos2, null, Color.Cyan * (0.5f + (float)Math.Sin(timer * 0.05f) * 0.2f), -timer * 0.005f, smallRingRunes.Size() * 0.5f, 2, SpriteEffects.None, 0);
			spriteBatch.Draw(mediumRingRunes, pos2, null, Color.Cyan * (0.5f + (float)Math.Sin((timer + 15) * 0.05f) * 0.2f), timer * 0.005f, mediumRingRunes.Size() * 0.5f, 2, SpriteEffects.None, 0);
			spriteBatch.Draw(largeRingRunes, pos2, null, Color.Cyan * (0.5f + (float)Math.Sin((timer + 30) * 0.05f) * 0.2f), -timer * 0.005f, largeRingRunes.Size() * 0.5f, 2, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);

			return false;
		}
	}
}