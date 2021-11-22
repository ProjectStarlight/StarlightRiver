using Microsoft.Xna.Framework;
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
			var a = Vector2.Normalize(Helpers.Helper.ScreenSize);
			effect.Parameters["offset"].SetValue(Vector2.Zero);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);
		}

		public override void SpritebatchChangeBack()
		{
			var effect = Filters.Scene["HotspringWater"].GetShader().Shader;
			effect.Parameters["sampleTexture2"].SetValue(HotspringMapTarget.hotspringMapTarget);
			effect.Parameters["sampleTexture3"].SetValue(HotspringMapTarget.hotspringShineTarget);
			effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);
			//the multiply by 1.3 and 1.5 seem to fix the jittering when moving, seems to be tied to the 2 magic numbers in Visuals.HotspringMapTarget.cs
			effect.Parameters["offset"].SetValue(Vector2.Zero);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);
		}
	}
}
