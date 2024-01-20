using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Tiles.Underground;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Waters.WaterAddons
{
	class HotspringAddon : WaterAddon
	{
		public static ScreenTarget hotspringMapTarget = new(RenderMainTarget, () => HotspringFountainDummy.AnyOnscreen, 1);

		public static ScreenTarget hotspringBackShineTarget = new(RenderForegroundShine, () => HotspringFountainDummy.AnyOnscreen, 1, (a) => Main.waterTarget.Size());
		public static ScreenTarget hotspringFrontShineTarget = new(RenderBackgroundShine, () => HotspringFountainDummy.AnyOnscreen, 1, (a) => Main.instance.backWaterTarget.Size());

		public override bool Visible => HotspringFountainDummy.AnyOnscreen;

		public override Texture2D BlockTexture(Texture2D normal, int x, int y)
		{
			return normal;
		}

		private static void RenderMainTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, default);

			foreach (Dummy dummy in DummySystem.dummies.Where(n => n.active && n is HotspringFountainDummy))
			{
				(dummy as HotspringFountainDummy).DrawMap(Main.spriteBatch);
			}
		}

		private static void RenderForegroundShine(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/HotspringWaterMap", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

			for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
			{
				for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
				{
					var pos = new Vector2(i, j);

					// This is the offset for the BACKGROUND, which is the position of the FOREGROUND minus screen pos (why? because god is a cruel creature)
					if (!Main.drawToScreen)
						pos -= Main.sceneWaterPos - Main.screenPosition;

					Vector2 tsp = Main.screenPosition;

					spriteBatch.Draw(tex2, pos - new Vector2(tsp.X % tex2.Width, tsp.Y % tex2.Height), null, Color.White);
				}
			}
		}

		private static void RenderBackgroundShine(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/HotspringWaterMap", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

			for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
			{
				for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
				{
					var pos = new Vector2(i, j);

					// This is the offset for the FOREGROUND, which is the position of the WALL RT minus screen pos (why? because god is a cruel creature)
					if (!Main.drawToScreen)
						pos -= Main.sceneWallPos - Main.screenPosition;

					Vector2 tsp = Main.screenPosition;

					spriteBatch.Draw(tex2, pos - new Vector2(tsp.X % tex2.Width, tsp.Y % tex2.Height), null, Color.White);
				}
			}
		}

		public override void SpritebatchChange()
		{
			Effect effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			effect.Parameters["offset"].SetValue(Vector2.Zero);
			effect.Parameters["sampleTexture2"].SetValue(hotspringMapTarget.RenderTarget);
			effect.Parameters["sampleTexture3"].SetValue(hotspringFrontShineTarget.RenderTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);
		}

		public override void SpritebatchChangeBack()
		{
			Effect effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			effect.Parameters["offset"].SetValue(Vector2.Zero);
			effect.Parameters["sampleTexture2"].SetValue(hotspringMapTarget.RenderTarget);
			effect.Parameters["sampleTexture3"].SetValue(hotspringBackShineTarget.RenderTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);
		}
	}
}