using Microsoft.Xna.Framework.Graphics;
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

		public override void Load()
		{
			if (Main.dedServ)
				return;

			starsTarget = new(DrawStars, CheckIsActive, 1f);
			starsMap = new(DrawMap, CheckIsActive, 1f);

			On_Main.DrawInterface += DrawOverlay;
			stars = new("StarlightRiver/Assets/Misc/DotTell", updateStars);
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

		public delegate void DrawOverlayDelegate(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget);


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
					del(orig, self, gameTime, starsMap, starsTarget);
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
			Texture2D texB = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			sb.Draw(texB, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			sb.End();
			sb.Begin(default, default, SamplerState.LinearWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise1").Value;
			var color = new Color(100, 230, 220)
			{
				A = 0
			};

			var target = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

			color.G = 160;
			color.B = 255;
			Vector2 offset = Vector2.One * Main.GameUpdateCount * 0.2f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.01f) * 20;
			var source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			color.G = 220;
			color.B = 255;
			offset = Vector2.One * Main.GameUpdateCount * 0.4f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.005f) * 36;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			color.G = 255;
			color.B = 255;
			offset = Vector2.One * Main.GameUpdateCount * 0.6f + Vector2.One.RotatedBy(Main.GameUpdateCount * 0.021f) * 15;
			source = new Rectangle((int)offset.X, (int)offset.Y, tex.Width, tex.Height);
			sb.Draw(tex, target, source, color * 0.05f);

			sb.End();
			sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			stars.DrawParticles(sb);
		}

		/// <summary>
		/// The update method for the stardust in the background
		/// </summary>
		/// <param name="particle"></param>
		private static void updateStars(Particle particle)
		{
			particle.Timer--;
			particle.Position += particle.Velocity;
			particle.Position.Y += (float)Math.Sin(particle.Timer * 0.015f) * particle.StoredPosition.X;

			if (particle.Timer < 30)
				particle.Alpha = particle.Timer / 30f;
		}
	}
}
