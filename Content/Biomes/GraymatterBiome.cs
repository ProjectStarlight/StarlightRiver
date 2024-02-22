using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Biomes
{
	internal class GraymatterBiome : ModBiome
	{
		public static ScreenTarget hallucinationMap;
		public static ScreenTarget overHallucinationMap;

		/// <summary>
		/// Can be subscribed to to add additional hallucinatory areas to the screen
		/// </summary>
		public static Action<SpriteBatch> onDrawHallucinationMap;

		/// <summary>
		/// Can be subscribed to to add additional 'hallucinatory' objects that can only be seen in a hallucinatory area
		/// </summary>
		public static Action<SpriteBatch> onDrawOverHallucinationMap;

		/// <summary>
		/// Can be subscribed to for drawing hallucinatory tiles, seperated to be able to be called from one iteration
		/// for optimization.
		/// </summary>
		public static Action<SpriteBatch, int, int> onDrawOverPerTile;

		public static int fullscreenTimer = 0;

		public override void Load()
		{
			hallucinationMap = new(DrawHallucinationMap, () => IsBiomeActive(Main.LocalPlayer), 1);
			overHallucinationMap = new(DrawOverHallucinationMap, () => IsBiomeActive(Main.LocalPlayer), 1.1f);

			On_Main.DrawItemTextPopups += DrawAuras;
		}

		public override bool IsBiomeActive(Player player)
		{
			return player.ZoneCrimson; // TODO: Add variable for monolith later
		}

		public override void OnInBiome(Player player)
		{
			if (player == Main.LocalPlayer)
			{
				if (player.HasBuff(BuffID.Invisibility) && fullscreenTimer < 60)
					fullscreenTimer++;
				else if (fullscreenTimer > 0)
					fullscreenTimer--;
			}
		}

		public void DrawHallucinationMap(SpriteBatch spriteBatch)
		{
			onDrawHallucinationMap?.Invoke(spriteBatch);

			// Draw the screen overlay for when the player is actively standing on gray matter
			var glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;

			spriteBatch.Draw(glow, Main.LocalPlayer.Center - Main.screenPosition, null, new Color(1, 1, 1f, 0), 0, glow.Size() / 2f, Main.screenWidth / glow.Width * (fullscreenTimer / 20f), 0, 0);
			spriteBatch.Draw(glow, Main.LocalPlayer.Center - Main.screenPosition, null, new Color(1, 1, 1f, 0), 0, glow.Size() / 2f, Main.screenWidth / glow.Width * (fullscreenTimer / 20f), 0, 0);
		}

		public void DrawOverHallucinationMap(SpriteBatch spriteBatch)
		{
			onDrawOverHallucinationMap?.Invoke(spriteBatch);

			var pos = (Main.screenPosition / 16).ToPoint16();

			var width = Main.screenWidth / 16 + 1;
			var height = Main.screenHeight / 16 + 1;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Point16 target = pos + new Point16(x, y);
					onDrawOverPerTile.Invoke(spriteBatch, target.X, target.Y);
				}
			}
		}

		private void DrawAuras(On_Main.orig_DrawItemTextPopups orig, float scaleTarget)
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

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, shader, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default);
			}

			orig(scaleTarget);
		}
	}
}
