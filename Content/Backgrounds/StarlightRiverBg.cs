using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.BossRushSystem;
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
		public static float timer = 0;
		public static float yOrigin = 0;
		public static int forceActiveTimer;

		public static int starCount;
		public static Vector2 screenCenter;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			starsTarget = new(DrawStars, CheckIsActive, 1f);
			starsMap = new(DrawMap, CheckIsActive, 1f);

			starsTarget.allowOnMenu = true;
			starsMap.allowOnMenu = true;

			stars = new("StarlightRiver/Assets/Misc/StarParticle", UpdateStars, ParticleSystem.AnchorOptions.Screen);

			On_Main.DrawInterface += DrawOverlay;
			On_Main.UpdateMenu += UpdateOnMenu;
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
				if (forceActiveTimer > 0)
					return true;

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

		private void DrawOverlay(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			if (DrawOverlayEvent != null)
			{
				foreach (DrawOverlayDelegate del in DrawOverlayEvent.GetInvocationList())
				{
					del(Main.gameTimeCache, starsMap, starsTarget);
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
			Matrix wantedMatrix = Main.gameMenu ? Matrix.CreateScale(1 / Main.UIScale) : Matrix.Identity;

			Texture2D texB = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			sb.Draw(texB, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			sb.End();
			sb.Begin(default, default, SamplerState.LinearWrap, default, RasterizerState.CullNone, default, wantedMatrix);

			Texture2D tex = Assets.Noise.PerlinSparse.Value;
			Texture2D tex2 = Assets.Noise.PerlinNoise.Value;

			var color = new Color(100, 230, 220)
			{
				A = 0
			};

			var target = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

			if (Main.gameMenu)
			{
				//target.Width = (int)(target.Width / Main.UIScale);
				//target.Height = (int)(target.Height / Main.UIScale);
			}

			color.R = 10;
			color.G = 100;
			color.B = 255;
			Vector2 offset = Vector2.UnitX * timer * 0.2f;
			//offset.X += 0.05f * Main.screenPosition.X % target.Width;
			//offset.Y += 0.05f * Main.screenPosition.Y % target.Height;
			var source = new Rectangle((int)offset.X, (int)offset.Y, tex2.Width, tex2.Height);
			sb.Draw(tex2, target, source, color * 0.12f);
			sb.Draw(tex, target, source, new Color(0.03f, 0.04f, 0.06f, 0) * 0.8f);

			color.R = 150;
			color.G = 10;
			color.B = 220;
			color.A = 0;
			offset = Vector2.UnitX * timer * 0.3f;
			//offset.X += 0.1f * Main.screenPosition.X % target.Width;
			//offset.Y += 0.1f * Main.screenPosition.Y % target.Height;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex2.Width / 3, tex2.Height / 3);
			sb.Draw(tex, target, source, color * 0.1f);

			color.R = 10;
			color.G = 155;
			color.B = 255;
			color.A = 0;
			offset = Vector2.UnitX * timer * 0.4f;
			//offset.X += 0.15f * Main.screenPosition.X % target.Width;
			//offset.Y += 0.15f * Main.screenPosition.Y % target.Height;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex2.Width / 2, tex2.Height / 2);
			sb.Draw(tex, target, source, color * 0.1f);

			Effect riverBodyShader = ShaderLoader.GetShader("RiverBody").Value;

			if (riverBodyShader != null)
			{
				riverBodyShader.Parameters["body"].SetValue(Assets.EnergyTrail.Value);
				riverBodyShader.Parameters["mask"].SetValue(Assets.FireTrail.Value);
				riverBodyShader.Parameters["u_amplitude"].SetValue(Main.screenHeight / 8f);
				riverBodyShader.Parameters["u_offset"].SetValue(0.5f + Main.screenPosition.X * 0.2f / target.Width);

				sb.End();
				sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, default, RasterizerState.CullNone, riverBodyShader, wantedMatrix);

				float yOff = (Main.screenPosition.Y - yOrigin) * -0.2f;

				riverBodyShader.Parameters["u_time"].SetValue(timer * 0.0045f);
				riverBodyShader.Parameters["u_alpha"].SetValue(starOpacity * 0.3f);
				riverBodyShader.Parameters["u_resolution"].SetValue(new Vector2(Main.screenWidth, 700));
				riverBodyShader.Parameters["u_target_resolution"].SetValue(new Vector2(starsTarget.RenderTarget.Width, 700f / Main.screenHeight * starsTarget.RenderTarget.Height));
				sb.Draw(Assets.ShadowTrail.Value, new Rectangle(0, (int)yOff + Main.screenHeight / 2 - 350, target.Width, 700), Color.White);

				riverBodyShader.Parameters["u_time"].SetValue(timer * 0.003f);
				riverBodyShader.Parameters["u_alpha"].SetValue(starOpacity * 0.6f);
				riverBodyShader.Parameters["u_resolution"].SetValue(new Vector2(Main.screenWidth, 500));
				riverBodyShader.Parameters["u_target_resolution"].SetValue(new Vector2(starsTarget.RenderTarget.Width, 500f / Main.screenHeight * starsTarget.RenderTarget.Height));
				sb.Draw(Assets.ShadowTrail.Value, new Rectangle(0, (int)yOff + Main.screenHeight / 2 - 250, target.Width, 500), Color.White);

				sb.End();
				sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, wantedMatrix);
			}

			//stars.DrawParticles(sb);
			stars.DrawParticlesWithEffect(sb, ShaderLoader.GetShader("GlowingCustomParticle").Value);
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

				float yOff = (Main.screenPosition.Y - yOrigin) * -0.2f;
				float xOff = Main.screenPosition.X * 0.2f;
				particle.Position.Y = yOff + Main.screenHeight / 2f + (float)Math.Sin((particle.Position.X + xOff) / Main.screenWidth * 6.28f) * Main.screenHeight / 8f + particle.StoredPosition.Y;

				particle.Alpha = starOpacity;

				if (particle.Timer < 30)
					particle.Alpha = particle.Timer / 30f * starOpacity;

				particle.Rotation += 0.2f * particle.Scale;
			}
			else if (particle.Type == 1) // Intro
			{
				particle.Timer--;
				particle.Position += particle.Velocity;

				particle.Scale = (1f - particle.Timer / 60f) * 1f;
			}
			else if (particle.Type == 2) // Background
			{
				particle.Timer--;
				particle.StoredPosition += particle.Velocity;

				particle.Alpha = 0.5f + 0.2f * (particle.Scale / 0.2f);

				if (particle.Timer > 1480)
					particle.Alpha = (1600 - particle.Timer) / 120f * particle.Alpha;

				if (particle.Timer < 120)
					particle.Alpha = particle.Timer / 120f * particle.Alpha;

				if (particle.Timer == 1 && starCount > 0)
					starCount--;

				particle.Alpha *= starOpacity;

				float factor = 1f - particle.Scale / 0.2f * 1.2f;

				particle.Position = particle.StoredPosition + (screenCenter - particle.StoredPosition) * factor - Main.screenPosition;


				if (particle.Timer > 3 && (particle.Position.X < -120 || particle.Position.X > Main.screenWidth + 120 || particle.Position.Y < -120 || particle.Position.Y > Main.screenHeight + 120))
					particle.Timer = 3;

				float grace = 60;

				if (particle.Position.X < -grace)
					particle.StoredPosition.X += Main.screenWidth + grace + 2 * ((screenCenter.X - particle.StoredPosition.X) * factor);

				if (particle.Position.X > Main.screenWidth + grace)
					particle.StoredPosition.X -= Main.screenWidth + grace + -2 * ((screenCenter.X - particle.StoredPosition.X) * factor);

				if (particle.Position.Y < -grace)
					particle.StoredPosition.Y += Main.screenHeight + grace + 2 * ((screenCenter.Y - particle.StoredPosition.Y) * factor);

				if (particle.Position.Y > Main.screenHeight + grace)
					particle.StoredPosition.Y -= Main.screenHeight + grace + -2 * ((screenCenter.Y - particle.StoredPosition.Y) * factor);
			}
		}

		public void SpawnAStar()
		{
			float weight = MathF.Pow(Main.rand.NextFloat(), 4);
			float scale = 0.05f + 0.15f * weight;
			int rangeX = (int)((1f - weight * 1.2f) * (Main.screenWidth));
			int rangeY = (int)((1f - weight * 1.2f) * (Main.screenHeight));

			var pos = new Vector2(Main.rand.Next(-rangeX, Main.screenWidth + rangeX), Main.rand.Next(-rangeY, Main.screenHeight + rangeY));

			var color = new Color(0f + (1f - scale / 0.2f) * 0.25f, 0.2f + Main.rand.NextFloat(0.3f) + scale / 0.2f * 0.1f, 1f, 0.25f);

			stars.AddParticle(pos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.05f, 0.1f) * (scale / 0.2f), Main.rand.NextBool() ? 0f : 1.57f / 2f, scale, color * 0.7f, 1600, pos + Main.screenPosition, new Rectangle(0, scale > 0.15f ? 120 : 0, 120, 120), 1, 2);
			starCount++;
		}

		public override void PostUpdateEverything()
		{
			if (!CheckIsActive() || Main.dedServ)
			{
				return;
			}

			screenCenter = Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;

			if (!Main.gameMenu && !BossRushSystem.isBossRush)
				yOrigin = Main.spawnTileY * 16 - 2400;
			else
				yOrigin = Main.screenPosition.Y;

			timer++;

			/*if (Main.rand.NextBool(2))
				SpawnAStar();*/

			if (starCount < 1100)
			{
				for (int k = 0; k < 3; k++)
				{
					SpawnAStar();
				}
			}

			if (starCount < 1200 && Main.rand.NextBool())
			{
				SpawnAStar();
			}

			if (Main.rand.NextBool(2, 3))
			{
				float prog = Main.rand.NextFloat();
				var starColor = new Color(0.1f, 0.2f + prog * 0.4f, 1f, 0.5f);

				bool star = Main.rand.NextBool(24);
				float partScale = Main.rand.NextFloat(0.05f, 0.15f) * (0.8f + prog * 0.2f);
				Color partColor = starColor * (0.8f + prog * 0.2f);

				if (star)
				{
					partScale += 0.15f;
					partColor *= 1.2f;
					partColor.A = 255;
				}

				stars.AddParticle(new Vector2(0, Main.screenHeight * 0.2f + prog * 0f), new Vector2(Main.rand.NextFloat(1f, 5f) + prog * 4f, 1), 0, partScale, partColor, 600, Vector2.One * (-70 + prog * 140f), new Rectangle(0, star ? 120 : 0, 120, 120), 1f);
			}

			stars.UpdateParticles();

			forceActiveTimer--;
		}

		private void UpdateOnMenu(On_Main.orig_UpdateMenu orig)
		{
			orig();

			PostUpdateEverything();
		}
	}
}