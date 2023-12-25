using StarlightRiver.Core.Systems.ScreenTargetSystem;
using StarlightRiver.Helpers;
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
		/// The maximum amount of rays to scan to generate light, will auto-adjust based on performance
		/// </summary>
		public static int maxScans;

		/// <summary>
		/// RenderTarget which holds the occluded lighting map
		/// </summary>
		public static ScreenTarget lightTarget = new(DrawLights, () => Main.LocalPlayer.HasBuff<HorrorDarkness>(), 1);

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
				Effect effect = Filters.Scene["StarMap"].GetShader().Shader;
				effect.Parameters["map"].SetValue(lightTarget.RenderTarget);
				effect.Parameters["background"].SetValue(Main.screenTarget);

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

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/Light").Value;
			float texLen = tex.Size().Length();

			Texture2D clearTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
			spriteBatch.Draw(clearTex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

			maxScans = 160;

			for (int k = 0; k < maxScans; k++)
			{
				float rot = k / (float)maxScans * 6.28f;

				Vector2 endPoint = player.Center + Vector2.UnitX.RotatedBy(rot) * 160 * 8;

				for (int i = 0; i < 160; i++)
				{
					Vector2 posCheck = player.Center + Vector2.UnitX.RotatedBy(rot) * i * 8;

					if (Helper.PointInTile(posCheck) || i == 159)
					{
						endPoint = posCheck;
						break;
					}
				}

				float dist = Vector2.Distance(player.Center, endPoint) + 4;
				float texDist = dist / (160 * 8f) * texLen;
				var source = new Rectangle(0, 0, (int)texDist, tex.Height);
				var target = new Rectangle(0, 0, (int)dist, tex.Height);
				target.Offset((player.Center - Main.screenPosition).ToPoint());

				Color color = Color.White;
				color.A = 0;

				spriteBatch.Draw(tex, target, source, color, rot, new Vector2(0, tex.Height / 2f), 0, 0);

				Texture2D impactTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowAlpha").Value;
				spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color * (1 - dist / (160 * 8f)) * 0.3f, 0, impactTex.Size() / 2, 1 - dist / (160 * 8f), 0, 0);
			}
		}
	}
}
