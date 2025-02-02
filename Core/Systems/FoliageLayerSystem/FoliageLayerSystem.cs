using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;

namespace StarlightRiver.Core.Systems.FoliageLayerSystem
{
	internal class FoliageLayerSystem : ModSystem
	{
		public static List<DrawData> overTilesData = [];
		public static List<DrawData> underTilesData = [];

		public static BlendState CeilBlend = new()
		{
			AlphaBlendFunction = BlendFunction.Max,
			ColorBlendFunction = BlendFunction.Max,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One
		};

		public override void Load()
		{
			On_Main.DoDraw_WallsTilesNPCs += DrawUnderTileLayer;
		}

		private void DrawUnderTileLayer(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
		{
			if (underTilesData.Count > 0)
			{
				DrawBlacks(Main.spriteBatch, underTilesData);
				Main.spriteBatch.End();

				Main.spriteBatch.Begin(default, CeilBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				DrawReals(Main.spriteBatch, underTilesData);
				Main.spriteBatch.End();

				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				underTilesData.Clear();
			}

			orig(self);
		}

		public override void PostDrawTiles()
		{
			if (overTilesData.Count > 0)
			{
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				DrawBlacks(Main.spriteBatch, overTilesData);
				Main.spriteBatch.End();

				Main.spriteBatch.Begin(default, CeilBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				DrawReals(Main.spriteBatch, overTilesData);
				Main.spriteBatch.End();

				overTilesData.Clear();
			}
		}

		private void DrawBlacks(SpriteBatch spriteBatch, List<DrawData> datas)
		{
			foreach (DrawData data in datas)
			{
				DrawData black = data;
				black.color = Color.Black;
				black.position -= Main.screenPosition;
				black.Draw(spriteBatch);
			}
		}

		private void DrawReals(SpriteBatch spriteBatch, List<DrawData> datas)
		{
			foreach (DrawData data in datas)
			{
				DrawData posed = data;
				posed.position -= Main.screenPosition;
				posed.Draw(spriteBatch);
			}
		}
	}
}
