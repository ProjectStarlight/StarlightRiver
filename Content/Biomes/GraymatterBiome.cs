using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Biomes
{
	internal class GraymatterBiome : ModBiome
	{
		public static ScreenTarget hallucinationMap;
		public static ScreenTarget overHallucinationMap;

		public override void Load()
		{
			hallucinationMap = new(DrawHallucinationMap, () => IsBiomeActive(Main.LocalPlayer), 1);
			overHallucinationMap = new(DrawOverHallucinationMap, () => IsBiomeActive(Main.LocalPlayer), 1.1f);

			On_Main.DrawInterface += DrawAuras;
		}

		public override bool IsBiomeActive(Player player)
		{
			return player.ZoneCrimson; // TODO: Add variable for monolith later
		}

		public void DrawHallucinationMap(SpriteBatch spriteBatch)
		{
			var pos = (Main.screenPosition / 16).ToPoint16();

			var width = Main.screenWidth / 16 + 1;
			var height = Main.screenHeight / 16 + 1;

			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					Point16 target = pos + new Point16(x, y);

					if (Framing.GetTileSafely(target).TileType == ModContent.TileType<GrayMatter>())
					{
						var glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
						Vector2 drawPos = target.ToVector2() * 16 + Vector2.One * 8 - Main.screenPosition;
						var color = Color.White;
						color.A = 0;

						spriteBatch.Draw(glow, drawPos, null, color, 0, glow.Size() / 2f, 1.5f, 0, 0);						
					}
				}
			}
		}

		public void DrawOverHallucinationMap(SpriteBatch spriteBatch)
		{

		}

		private void DrawAuras(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			if (IsBiomeActive(Main.LocalPlayer))
			{
				Effect shader = Filters.Scene["GrayMatter"].GetShader().Shader;
				var noise = ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/SwirlyNoiseLooping").Value;

				shader.Parameters["background"].SetValue(Main.screenTarget);
				shader.Parameters["map"].SetValue(hallucinationMap.RenderTarget);
				shader.Parameters["noise"].SetValue(noise);
				shader.Parameters["over"].SetValue(overHallucinationMap.RenderTarget);
				shader.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
				shader.Parameters["screensize"].SetValue(noise.Size() / new Vector2(Main.screenWidth, Main.screenHeight));

				Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, shader);

				Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);

				Main.spriteBatch.End();
			}

			orig(self, gameTime);
		}
	}
}
