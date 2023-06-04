using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Tiles.Underground;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Waters.WaterAddons
{
	class HotspringAddon : WaterAddon
	{
		public static ScreenTarget hotspringMapTarget = new(RenderMainTarget, () => HotspringFountainDummy.AnyOnscreen, 1);
		public static ScreenTarget hotspringShineTarget = new(RenderSecondaryTarget, () => HotspringFountainDummy.AnyOnscreen, 1);

		public Vector2 oldScreenPos;

		public override bool Visible => HotspringFountainDummy.AnyOnscreen;

		public override Texture2D BlockTexture(Texture2D normal, int x, int y)
		{
			return normal;
		}

		private static void RenderMainTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);

			for (int k = 0; k < Main.maxProjectiles; k++)
			{
				Projectile proj = Main.projectile[k];

				if (proj.active && proj.ModProjectile is HotspringFountainDummy)
					(proj.ModProjectile as HotspringFountainDummy).DrawMap(Main.spriteBatch);
			}
		}

		private static void RenderSecondaryTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/HotspringWaterMap", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

			//The seam issue is not in this file, See StarlightRiver.cs and enable the commented out PostDrawInterface hook to view RTs
			for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
			{
				for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
				{
					//the divide by 1.3 and 1.5 are what keep the tile tied to the world location, seems to be tied to the 2 magic numbers in HotspringAddon.cs
					var pos = new Vector2(i, j);
					spriteBatch.Draw(tex2, pos - new Vector2(Main.screenPosition.X % tex2.Width, Main.screenPosition.Y % tex2.Height), null, Color.White);
				}
			}
		}

		public override void SpritebatchChange()
		{
			oldScreenPos = Main.screenPosition;

			Effect effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			//effect.Parameters["offset"].SetValue((Main.screenPosition - oldScreenPos) * -1);
			effect.Parameters["sampleTexture2"].SetValue(hotspringMapTarget.RenderTarget);
			effect.Parameters["sampleTexture3"].SetValue(hotspringShineTarget.RenderTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void SpritebatchChangeBack()
		{
			Effect effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			//effect.Parameters["offset"].SetValue((Main.screenPosition - oldScreenPos) * -1);
			effect.Parameters["sampleTexture2"].SetValue(hotspringMapTarget.RenderTarget);
			effect.Parameters["sampleTexture3"].SetValue(hotspringShineTarget.RenderTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}