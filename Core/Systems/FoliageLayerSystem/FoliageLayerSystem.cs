using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;

namespace StarlightRiver.Core.Systems.FoliageLayerSystem
{
	internal class FoliageLayerSystem : ModSystem
	{
		public static List<DrawData> data = [];

		public static BlendState CeilBlend = new()
		{
			AlphaBlendFunction = BlendFunction.Max,
			ColorBlendFunction = BlendFunction.Max,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One
		};

		public override void PostDrawTiles()
		{
			DrawBlacks(Main.spriteBatch);
			DrawReals(Main.spriteBatch);

			data.Clear();
		}

		private void DrawBlacks(SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			foreach (DrawData data in data)
			{
				DrawData black = data;
				black.color = Color.Black;
				black.position -= Main.screenPosition;
				black.Draw(spriteBatch);
			}

			spriteBatch.End();
		}

		private void DrawReals(SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(default, CeilBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			foreach (DrawData data in data)
			{
				DrawData posed = data;
				posed.position -= Main.screenPosition;
				posed.Draw(spriteBatch);
			}

			spriteBatch.End();
		}
	}
}
