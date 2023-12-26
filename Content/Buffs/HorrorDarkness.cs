using StarlightRiver.Core.Systems.ScreenTargetSystem;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Buffs
{
	internal class HorrorDarkness : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public HorrorDarkness() : base("Horrific Darkness", "Nowhere to hide...", true, false) { }
	}

	internal class HorrorDarknessSystem : ModSystem
	{
		/// <summary>
		/// The maximum amount of rays to scan to generate light
		/// </summary>
		public static int slices = 200;

		/// <summary>
		/// The distance each ray marches per step
		/// </summary>
		public static int resolution = 4;

		/// <summary>
		/// The max radius of the desired light
		/// </summary>
		public static int radius = 640;

		/// <summary>
		/// RenderTarget which holds the occluded lighting map
		/// </summary>
		public static ScreenTarget lightTarget = new(DrawLights, () => Main.LocalPlayer.HasBuff<HorrorDarkness>(), 1);

		/// <summary>
		/// Total amount of checks per reay
		/// </summary>
		public static int Polls => 640 / resolution;

		/// <summary>
		/// If drawing should occur or not, based on if the RenderTarget is active
		/// </summary>
		public static bool Active => lightTarget.activeFunct();

		public override void Load()
		{
			On_Main.DrawInterface += DrawBlackout;
		}

		private void DrawBlackout(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			if (Active && lightTarget.RenderTarget != null)
			{
				Effect effect = Filters.Scene["HorrorLight"].GetShader().Shader;
				effect.Parameters["map"].SetValue(lightTarget.RenderTarget);
				effect.Parameters["background"].SetValue(Main.screenTarget);
				effect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
				effect.Parameters["rad"].SetValue(4);

				Main.spriteBatch.Begin();
				Texture2D clearTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
				Main.spriteBatch.Draw(clearTex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
				Main.spriteBatch.End();

				Main.spriteBatch.Begin(default, default, default, default, default, effect);

				Color color = Color.White;
				color.A = 0;

				Main.spriteBatch.Draw(lightTarget.RenderTarget, Vector2.Zero, color);
				Main.spriteBatch.End();
			}

			orig(self, gameTime);
		}

		/// <summary>
		/// Draws the radial beams eminating from the player, up to tiles
		/// </summary>
		/// <param name="spriteBatch">The SpriteBatch to draw with</param>
		public static void DrawLights(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;

			Texture2D clearTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
			spriteBatch.Draw(clearTex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			float[] dists = Enumerable.Repeat((float)Polls * resolution, slices).ToArray();

			for (int k = 0; k < slices; k++)
			{
				float rot = k / (float)slices * 6.28f;

				for (int i = 0; i < Polls; i++)
				{
					Vector2 posCheck = player.Center + Vector2.UnitX.RotatedBy(rot) * i * resolution;

					if (Helper.PointInTile(posCheck))
					{
						Vector2 endPoint = posCheck;
						dists[k] = Vector2.Distance(player.Center, endPoint) + 4;
						break;
					}
				}			
			}

			for (int k = 1; k < slices - 1; k++)
			{
				float dist = dists[k];

				DrawSlice(spriteBatch, dist, k / (float)slices * 6.28f);
				int synth = 8;
				for (int i = 1; i < synth; i++)
				{
					float percent = (float)i / synth;
					DrawSlice(spriteBatch, Helper.LerpFloat(dist, dists[k - 1], percent), (k - percent) / (float)slices * 6.28f);
					DrawSlice(spriteBatch, Helper.LerpFloat(dist, dists[k + 1], percent), (k + percent) / (float)slices * 6.28f);
				}
			}
		}

		public static void DrawSlice(SpriteBatch spriteBatch, float dist, float rot)
		{
			Player player = Main.LocalPlayer;

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/Light").Value;
			float texLen = tex.Size().Length();

			float texDist = dist / ((float)Polls * resolution) * texLen;
			var source = new Rectangle(0, 0, (int)texDist, tex.Height);
			var target = new Rectangle(0, 0, (int)dist, tex.Height);
			target.Offset((player.Center - Main.screenPosition).ToPoint());

			Color color = Color.White;
			color.A = 0;

			spriteBatch.Draw(tex, target, source, color * 0.1f, rot, new Vector2(0, tex.Height / 2f), 0, 0);
		}
	}
}
