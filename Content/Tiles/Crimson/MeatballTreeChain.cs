using Microsoft.VisualBasic;
using StarlightRiver.Content.Tiles.BaseTypes;
using StarlightRiver.Core.Systems.FoliageLayerSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class MeatballTreeChain : PhysicsChain
	{
		public MeatballTreeChain()
		{
			segmentLength = 28;
			segmentLengthMultiplier = 1.5f;
		}

		public override void PerPointDraw(SpriteBatch spriteBatch, Vector2 worldPos, Vector2 nextPos, Vector2 prevPos, int index, int length)
		{
			segmentLength = 32;
			segmentLengthMultiplier = 1.3f;

			Texture2D tex = Assets.Tiles.Crimson.MeatballTreeChain.Value;

			int variant = 3;

			if (index == 0 || index == length - 2)
			{
				return;
			}

			if (length >= 8)
			{
				if (index < 5)
					variant = index - 1;

				if (index >= length - 6)
					variant = length - 3 - index;
			}

			var frame = new Rectangle(variant * 34, 0, 32, 32);

			var rot = index > length / 2 ? nextPos.DirectionTo(worldPos).ToRotation() : prevPos.DirectionTo(nextPos).ToRotation();
			FoliageLayerSystem.overTilesData.Add(new(tex, worldPos, frame, new Color(Lighting.GetSubLight(worldPos)), rot, frame.Size() / 2f, 1f, 0, 0));
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					Tile tile = Framing.GetTileSafely(i + x, j + y);
					if (tile.HasTile && tile.TileType == ModContent.TileType<MeatballTreeTopper>())
					{
						return false;
					}
				}
			}

			WorldGen.KillTile(i, j);

			return false;
		}
	}
}