using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.Items.Food.Special;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.BossRushSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using Terraria.Graphics.Effects;

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

		public override void Load()
		{
			if (Main.dedServ)
				return;

			starsTarget = new(DrawStars, CheckIsActive, 1f);
			starsMap = new(DrawMap, CheckIsActive, 1f);

			starsTarget.allowOnMenu = true;
			starsMap.allowOnMenu = true;

			stars = new("StarlightRiver/Assets/Misc/StarParticle", UpdateStars, ParticleSystem.AnchorOptions.Screen);

			ScreenspaceShaderSystem.AddScreenspacePass(new(1, DrawOverlay, CheckIsActive));
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
		public void DrawOverlay(SpriteBatch spriteBatch, Texture2D screen)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(screen, Vector2.Zero, Color.White);
			spriteBatch.End();

			if (DrawOverlayEvent != null)
			{
				foreach (DrawOverlayDelegate del in DrawOverlayEvent.GetInvocationList())
				{
					del(Main.gameTimeCache, starsMap, starsTarget);
				}
			}
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

			color.G = 190;
			color.B = 255;
			Vector2 offset = Vector2.UnitX * timer * 0.2f;
			//offset.X += 0.05f * Main.screenPosition.X % target.Width;
			//offset.Y += 0.05f * Main.screenPosition.Y % target.Height;
			var source = new Rectangle((int)offset.X, (int)offset.Y, tex2.Width, tex2.Height);
			sb.Draw(tex2, target, source, color * 0.05f);

			color.R = 150;
			color.G = 10;
			color.B = 255;
			color.A = 0;
			offset = Vector2.UnitX * timer * 0.3f;
			//offset.X += 0.1f * Main.screenPosition.X % target.Width;
			//offset.Y += 0.1f * Main.screenPosition.Y % target.Height;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex2.Width, tex2.Height);
			sb.Draw(tex2, target, source, color * 0.05f);

			color.R = 10;
			color.G = 255;
			color.B = 255;
			color.A = 0;
			offset = Vector2.UnitX * timer * 0.4f;
			//offset.X += 0.15f * Main.screenPosition.X % target.Width;
			//offset.Y += 0.15f * Main.screenPosition.Y % target.Height;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex2.Width, tex2.Height);
			sb.Draw(tex2, target, source, color * 0.05f);

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
				sb.Draw(Assets.ShadowTrail.Value, new Rectangle(0, (int)yOff + Main.screenHeight / 2 - 350, target.Width, 700), Color.White);

				riverBodyShader.Parameters["u_time"].SetValue(timer * 0.003f);
				riverBodyShader.Parameters["u_alpha"].SetValue(starOpacity * 0.6f);
				riverBodyShader.Parameters["u_resolution"].SetValue(new Vector2(Main.screenWidth, 500));
				sb.Draw(Assets.ShadowTrail.Value, new Rectangle(0, (int)yOff + Main.screenHeight / 2 - 250, target.Width, 500), Color.White);

				sb.End();
				sb.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, wantedMatrix);
			}

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

				//particle.Rotation += 0.02f;

				particle.Alpha = 0.5f + 0.5f * (particle.Scale / 0.35f);

				if (particle.Timer > 1570)
					particle.Alpha = (1600 - particle.Timer) / 30f * particle.Alpha;

				if (particle.Timer < 30)
					particle.Alpha = particle.Timer / 30f * particle.Alpha;

				particle.Alpha *= starOpacity;

				particle.Position = particle.StoredPosition + (particle.StoredPosition - (Main.screenPosition + Main.ScreenSize.ToVector2() / 2f)) * (particle.Scale * 6f - 0.35f * 3f) - Main.screenPosition;
			}
		}

		public override void PostUpdateEverything()
		{
			if (!CheckIsActive() || Main.dedServ)
				return;

			if (!Main.gameMenu && !BossRushSystem.isBossRush)
				yOrigin = (int)Main.spawnTileY * 16 - 2400;
			else
				yOrigin = Main.screenPosition.Y;

			timer++;

			if (Main.rand.NextBool(2))
			{
				var pos = new Vector2(Main.rand.Next(-Main.screenWidth / 2, (int)(Main.screenWidth * 1.5f)), Main.rand.Next(-Main.screenHeight / 2, (int)(Main.screenHeight * 1.5f)));
				float scale = Main.rand.NextFloat(0.35f);

				var color = new Color(0.1f, 0.4f + Main.rand.NextFloat(0.1f) + scale / 0.35f * 0.5f, 1f)
				{
					A = 0
				};

				stars.AddParticle(pos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.05f, 0.2f) * (scale / 0.35f), 0, scale, color * 0.8f, 1600, pos + Main.screenPosition, new Rectangle(0, 120, 120, 120), 1, 2);
			}

			if (Main.rand.NextBool(2, 3))
			{
				float prog = Main.rand.NextFloat();
				var starColor = new Color(0.1f, 0.2f + prog * 0.6f, 1f)
				{
					A = 0
				};

				bool star = Main.rand.NextBool(24);
				float partScale = Main.rand.NextFloat(0.05f, 0.15f) * (0.8f + prog * 0.2f);
				Color partColor = starColor * (0.6f + prog * 0.2f);

				if (star)
				{
					partScale += 0.15f;
					partColor *= 1.5f;
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