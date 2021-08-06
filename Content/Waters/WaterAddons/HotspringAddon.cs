using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Tiles.Underground;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Waters.WaterAddons
{
	class HotspringAddon : WaterAddon
	{
		public override bool Visible => HotspringFountainDummy.AnyOnscreen;

		public override Texture2D BlockTexture(Texture2D normal, int x, int y)
		{
			return normal;
		}

		public override void SpritebatchChange()
		{
			/*Main.spriteBatch.Begin();
			Main.spriteBatch.Draw(HotspringMapTarget.hotspringShineTarget, Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.White);
			Main.spriteBatch.End();*/

			var effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			effect.Parameters["sampleTexture2"].SetValue(HotspringMapTarget.hotspringMapTarget);
			effect.Parameters["sampleTexture3"].SetValue(HotspringMapTarget.hotspringShineTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);
			effect.Parameters["offset"].SetValue((Main.screenPosition - Main.sceneWaterPos) / Helpers.Helper.ScreenSize);

			Main.spriteBatch.Begin(default, default, default, default, default, effect);
		}

		public override void SpritebatchChangeBack()
		{
			var effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			effect.Parameters["sampleTexture2"].SetValue(HotspringMapTarget.hotspringMapTarget);
			effect.Parameters["sampleTexture3"].SetValue(HotspringMapTarget.hotspringShineTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);
			effect.Parameters["offset"].SetValue((Main.screenPosition - Main.sceneBackgroundPos) / Helpers.Helper.ScreenSize);

			Main.spriteBatch.Begin(default, default, default, default, default, effect);
		}
	}
}
