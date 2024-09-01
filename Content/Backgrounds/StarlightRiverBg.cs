using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;

namespace StarlightRiver.Content.Backgrounds
{
	class StarlightRiverBackground : ModSystem
	{
		/// <summary>
		/// Holds the drawing of the background
		/// </summary>
		public static ScreenTarget starsTarget;
		public static ScreenTarget starsMap;
		public static ParticleSystem stars;

		public static float starOpacity = 1;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			starsTarget = new(DrawStars, CheckIsActive, 1f);
			starsMap = new(DrawMap, CheckIsActive, 1f);
			stars = new("StarlightRiver/Assets/Misc/StarParticle", UpdateStars, ParticleSystem.AnchorOptions.Screen);

			On_Main.DrawInterface += DrawOverlay;
		}

		public override void Unload()
		{
			CheckIsActiveEvent = null;
			DrawOverlayEvent = null;
			DrawMapEvent = null;

			starsTarget = null;
			starsMap = null;
			stars = null;
		}

		public delegate bool CheckIsActiveDelegate();
		public static event CheckIsActiveDelegate CheckIsActiveEvent;
		public bool CheckIsActive()
		{
			if (CheckIsActive != null)
			{
				bool isActive = false;
				foreach (CheckIsActiveDelegate del in CheckIsActiveEvent.GetInvocationList())
				{
					isActive |= del();
				}

				return isActive;
			}

			return false;
		}

		public delegate void DrawOverlayDelegate(GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget);

		/// <summary>
		/// Event handler for drawing the background -over- the rest of the game
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="gameTime"></param>
		/// <param name="starsMap"></param>
		/// <param name="starsTarget"></param>
		public static event DrawOverlayDelegate DrawOverlayEvent;
		public void DrawOverlay(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			if (DrawOverlayEvent != null)
			{
				foreach (DrawOverlayDelegate del in DrawOverlayEvent.GetInvocationList())
				{
					del(gameTime, starsMap, starsTarget);
				}
			}

			orig(self, gameTime);
		}

		public delegate void DrawMapDelegate(SpriteBatch sb);

		/// <summary>
		/// Event handler for drawing the map for where the background should appear.
		/// </summary>
		/// <param name="sb"></param>
		public static event DrawMapDelegate DrawMapEvent;
		public static void DrawMap(SpriteBatch sb)
		{
			if (DrawMapEvent != null)
			{
				foreach (DrawMapDelegate del in DrawMapEvent.GetInvocationList())
				{
					del(sb);
				}
			}
		}

		/// <summary>
		/// Draws the background to a ScreenTarget to be used later
		/// </summary>
		/// <param name="sb"></param>
		public static void DrawStars(SpriteBatch sb)
		{
			Texture2D texB = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			sb.Draw(texB, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			sb.End();
			sb.Begin(default, default, SamplerState.LinearWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

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
			Vector2 offset = Vector2.One * Main.GameUpdateCount * 0.2f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.01f) * 20;
			var source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			color.R = 150;
			color.G = 10;
			color.B = 255;
			color.A = 0;
			offset = Vector2.One * Main.GameUpdateCount * 0.4f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.005f) * 36;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex2, target, source, color * 0.05f);

			color.R = 10;
			color.G = 255;
			color.B = 255;
			color.A = 0;
			offset = Vector2.One * Main.GameUpdateCount * 0.6f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.021f) * 15;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex3, target, source, color * 0.05f);

			sb.End();
			sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			stars.DrawParticles(sb);
		}

		/// <summary>
		/// The update method for the stardust in the background
		/// </summary>
		/// <param name="particle"></param>
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

		public override void PostUpdateEverything()
		{
			if (!CheckIsActive() || Main.dedServ)
				return;

			// star particles
			var starColor = new Color(150, Main.rand.Next(150, 255), 255)
			{
				A = 0
			};

			if (Main.rand.NextBool(2))
			{
				var pos = new Vector2(Main.rand.Next(-Main.screenWidth / 2, (int)(Main.screenWidth * 1.5f)), Main.rand.Next(-Main.screenHeight / 2, (int)(Main.screenHeight * 1.5f)));
				stars.AddParticle(new Particle(pos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.25f), 0, Main.rand.NextFloat(0.35f), starColor * 0.8f, 1600, pos + Main.screenPosition, new Rectangle(0, 120, 120, 120), 1, 2));
			}

			float prog = Main.rand.NextFloat();
			starColor.G = (byte)(150 + prog * 105);

			bool star = Main.rand.NextBool(18);
			stars.AddParticle(new Particle(new Vector2(0, Main.screenHeight * 0.2f + prog * 0f), new Vector2(3f + prog * 4f, 1), 0, star ? Main.rand.NextFloat(0.2f, 0.3f) : Main.rand.NextFloat(0.05f, 0.1f), starColor * (star ? 1.2f : 1f), 600, Vector2.One * (prog * 110f), new Rectangle(0, star ? 120 : 0, 120, 120), 1));
		}
	}
}