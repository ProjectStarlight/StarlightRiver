using StarlightRiver.Core.Systems.DummyTileSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	public abstract class ShrineDummy : Dummy
	{
		public const float SHRINE_STATE_IDLE = 0;
		public const float SHRINE_STATE_ACTIVE = 1;
		public const float SHRINE_STATE_FAILED = -1;
		public const float SHRINE_STATE_DEFEATED = -2;

		public float timer;
		public float state;

		public abstract int ArenaOffsetX { get; }
		public abstract int ArenaSizeX { get; }
		public abstract int ArenaOffsetY { get; }
		public abstract int ArenaSizeY { get; }

		public abstract int ShrineTileWidth { get; }
		public abstract int ShrineTileHeight { get; }

		public Rectangle ArenaPlayer => new((ParentX + ArenaOffsetX) * 16, (ParentY + ArenaOffsetY) * 16, ArenaSizeX * 16, ArenaSizeY * 16);
		public Rectangle ArenaTile => new(ParentX + ArenaOffsetX, ParentY + ArenaOffsetY, ArenaSizeX, ArenaSizeY);

		public ShrineDummy(int validType, int width, int height) : base(validType, width, height) { }

		protected void SetFrame(int frame)
		{
			for (int x = 0; x < ShrineTileWidth; x++)
			{
				for (int y = 0; y < ShrineTileHeight; y++)
				{
					int realX = ParentX - ShrineTileWidth / 2 + x; //intentionally integer division for rounddown
					int realY = ParentY - ShrineTileHeight / 2 + y;

					Framing.GetTileSafely(realX, realY).TileFrameX = (short)((x + frame * ShrineTileWidth) * 18);
				}
			}

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendTileSquare(-1, ParentX, ParentY, ShrineTileWidth - ShrineTileWidth / 2, ShrineTileHeight - ShrineTileHeight / 2, TileChangeType.None);
				netUpdate = true;
			}
		}
	}
}