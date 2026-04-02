using StarlightRiver.Content.Tiles.BaseTypes;
using StarlightRiver.Core.Systems.FoliageLayerSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class MeatballTreeChain : PhysicsChain
	{
		public MeatballTreeChain()
		{
			segmentLength = 16;
			segmentLengthMultiplier = 1.3f;
		}

		public override void PerPointDraw(SpriteBatch spriteBatch, Vector2 worldPos, Vector2 nextPos, Vector2 prevPos, int index, int length)
		{
			Texture2D tex = Assets.Tiles.Crimson.MeatballTreeChain.Value;

			int variant = 9;

			if (length >= 12)
			{
				if (index < 10)
					variant = index;

				if (index >= length - 11)
					variant = length - 2 - index;
			}

			var frame = new Rectangle(variant * 18, 0, 16, 32);

			var rot = index > length / 2 ? nextPos.DirectionTo(worldPos).ToRotation() : prevPos.DirectionTo(nextPos).ToRotation();
			FoliageLayerSystem.overTilesData.Add(new(tex, worldPos, frame, new Color(Lighting.GetSubLight(worldPos)), rot, frame.Size() / 2f, 1f, 0, 0));
		}

		public override void PerPointUpdate(Vector2 worldPos, Vector2 nextPos, Vector2 prevPos, int index, int length)
		{
			if (Main.rand.NextBool(150))
				Dust.NewDustPerfect(worldPos, DustID.Water_Crimson, Vector2.UnitX * Main.windSpeedCurrent * 5);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			bool startOK = false;
			bool endOK = false;

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					Tile startScan = Framing.GetTileSafely(i + x, j + y);
					Tile endScan = Framing.GetTileSafely(i + x, j + y);

					if (startScan.HasTile && startScan.TileType == ModContent.TileType<MeatballTreeTopper>())
						startOK = true;

					if (endScan.HasTile && endScan.TileType == ModContent.TileType<MeatballTreeTopper>())
						endOK = true;
				}
			}

			if (!startOK || !endOK)
				WorldGen.KillTile(i, j);

			return false;
		}
	}
}