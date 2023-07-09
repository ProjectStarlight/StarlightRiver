using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Underground
{
	public abstract class ShrineDummy : Dummy
	{
		public const float ShrineState_Idle = 0;
		public const float ShrineState_Active = 1;
		public const float ShrineState_Failed = -1;
		public const float ShrineState_Defeated = -2;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public abstract int ArenaOffsetX { get; }
		public abstract int ArenaSizeX { get; }
		public abstract int ArenaOffsetY { get; }
		public abstract int ArenaSizeY { get; }

		public abstract int ShrineTileWidth { get; }
		public abstract int ShrineTileHeight { get; }

		public Rectangle ArenaPlayer => new((ParentX + ArenaOffsetX) * 16, (ParentY + ArenaOffsetY) * 16, ArenaSizeX * 16, ArenaSizeY * 16);
		public Rectangle ArenaTile => new(ParentX + ArenaOffsetX, ParentY + ArenaOffsetY, ArenaSizeX, ArenaSizeY);

		public ShrineDummy(int validType, int width, int height) : base(validType, width, height) { }
	}
}
