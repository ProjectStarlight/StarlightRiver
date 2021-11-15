using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class GearTile : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<GearTileDummy>();

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.Debug;
			return base.Autoload(ref name, ref texture);
		}
	}

	class GearTileDummy : Dummy
	{
		bool engaged = false;
		int direction = 0;
		int size = 0;

		public GearTileDummy() : base(ModContent.TileType<GearTile>(), 16, 16) { }

		public void Engage()
		{
			if (size > 0)
			{
				engaged = true;

				Point16 pos = new Point16(ParentX, ParentY);

				switch(size)
				{
					case 1: //small gear

						//check VS smalls
						CheckCardinals(pos, 3, 1);
						CheckSubCardinals(pos, 2, 1);
						//check VS mediums
						CheckCardinals(pos, 4, 2);
						//check VS larges
						Check12Rad5(pos, 3);
						break;

					case 2: //medium gear

						//check VS smalls
						CheckCardinals(pos, 4, 1);
						//check VS mediums
						Check12Rad5(pos, 2);
						//check VS larges
						Check12Rad6(pos, 3);
						break;

					case 3: //large gear

						//check VS smalls
						Check12Rad5(pos, 1);
						//check VS mediums
						Check12Rad6(pos, 2);
						//check VS larges
						CheckCardinals(pos, 7, 3);
						CheckSubCardinals(pos, 5, 3);
						break;

					default: //fallback
						break;
				}
			}
		}

		private void CheckCardinals(Point16 pos, int radius, int size)
		{
			TryEngage(pos + new Point16(radius, 0), size);
			TryEngage(pos + new Point16(0, radius), size);
			TryEngage(pos + new Point16(-radius, 0), size);
			TryEngage(pos + new Point16(0, -radius), size);
		}

		private void CheckSubCardinals(Point16 pos, int radius, int size)
		{
			TryEngage(pos + new Point16(radius, radius), size);
			TryEngage(pos + new Point16(-radius, -radius), size);
			TryEngage(pos + new Point16(-radius, radius), size);
			TryEngage(pos + new Point16(radius, -radius), size);
		}

		private void Check12Rad5(Point16 pos, int size)
		{
			CheckCardinals(pos, 5, size);

			TryEngage(pos + new Point16(4, 3), size);
			TryEngage(pos + new Point16(3, 4), size);
			TryEngage(pos + new Point16(-3, 4), size);
			TryEngage(pos + new Point16(-4, 3), size);
			TryEngage(pos + new Point16(-4, -3), size);
			TryEngage(pos + new Point16(-3, -4), size);
			TryEngage(pos + new Point16(3, -4), size);
			TryEngage(pos + new Point16(4, -3), size);
		}

		private void Check12Rad6(Point16 pos, int size)
		{
			CheckCardinals(pos, 6, size);

			TryEngage(pos + new Point16(5, 3), size);
			TryEngage(pos + new Point16(3, 5), size);
			TryEngage(pos + new Point16(-3, 5), size);
			TryEngage(pos + new Point16(-5, 3), size);
			TryEngage(pos + new Point16(-5, -3), size);
			TryEngage(pos + new Point16(-3, -5), size);
			TryEngage(pos + new Point16(3, -5), size);
			TryEngage(pos + new Point16(5, -3), size);
		}

		private void TryEngage(Point16 pos, int size)
		{
			var tile = Framing.GetTileSafely(pos);
			if(tile.type == ModContent.TileType<GearTile>())
			{
				if (DummyTile.DummyExists(pos.X, pos.Y, ModContent.ProjectileType<GearTileDummy>()))
				{
					var gearDummy = (GearTileDummy)DummyTile.GetDummy(pos.X, pos.Y, ModContent.ProjectileType<GearTileDummy>()).modProjectile;

					if (gearDummy.size == size && !gearDummy.engaged)
					{
						gearDummy.Engage();
						gearDummy.direction = direction * -1;
					}
				}
			}
		}
	}
}
