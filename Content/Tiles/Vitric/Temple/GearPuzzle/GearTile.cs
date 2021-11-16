using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

		public override void SetDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, 1, 1, new Color(1, 1, 1));
		}

		public override bool NewRightClick(int i, int j)
		{
			if(Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				(Dummy(i, j).modProjectile as GearTileDummy).Toggle();
				return true;
			}

			(Dummy(i, j).modProjectile as GearTileDummy).Size++;

			return true;
		}
	}

	class GearTileDummy : Dummy
	{
		private int size;
		bool engaged = false;
		float direction = 0;

		public int Size
		{
			get => size;
			set => size = value % 4;
		}

		public int Teeth => GetTeeth();


		public GearTileDummy() : base(ModContent.TileType<GearTile>(), 16, 16) { }

		public int GetTeeth()
		{
			switch (size)
			{
				case 0: return 1;
				case 1: return 8;
				case 2: return 16;
				case 3: return 24;
				default: return 1;
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D tex;

			switch (size)
			{
				case 0: return;
				case 1: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearSmall"); break;
				case 2: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearMid"); break;
				case 3: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearLarge"); break;
				default: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "GearSmall"); break;
			}

			float rot = 0;

			if (engaged)
				rot = Main.GameUpdateCount * 0.01f * direction;

			if (direction > 0)
				rot += 0.2f;

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, rot, tex.Size() / 2, 1, 0, 0);
		}

		public void RecurseOverGears(Action<Point16, int> action)
		{
			if (size > 0)
			{
				Point16 pos = new Point16(ParentX, ParentY);

				switch (size)
				{
					case 1: //small gear

						//check VS smalls
						CheckCardinals(action, pos, 3, 1);
						CheckSubCardinals(action, pos, 2, 1);
						//check VS mediums
						CheckCardinals(action, pos, 4, 2);
						//check VS larges
						Check12Rad5(action, pos, 3);
						break;

					case 2: //medium gear

						//check VS smalls
						CheckCardinals(action, pos, 4, 1);
						//check VS mediums
						Check12Rad5(action, pos, 2);
						//check VS larges
						Check12Rad6(action, pos, 3);
						break;

					case 3: //large gear

						//check VS smalls
						Check12Rad5(action, pos, 1);
						//check VS mediums
						Check12Rad6(action, pos, 2);
						//check VS larges
						CheckCardinals(action, pos, 7, 3);
						CheckSubCardinals(action, pos, 5, 3);
						break;

					default: //fallback
						break;
				}
			}
		}

		private void CheckCardinals(Action<Point16, int> action, Point16 pos, int radius, int size)
		{
			action(pos + new Point16(radius, 0), size);
			action(pos + new Point16(0, radius), size);
			action(pos + new Point16(-radius, 0), size);
			action(pos + new Point16(0, -radius), size);
		}

		private void CheckSubCardinals(Action<Point16, int> action, Point16 pos, int radius, int size)
		{
			action(pos + new Point16(radius, radius), size);
			action(pos + new Point16(-radius, -radius), size);
			action(pos + new Point16(-radius, radius), size);
			action(pos + new Point16(radius, -radius), size);
		}

		private void Check12Rad5(Action<Point16, int> action, Point16 pos, int size)
		{
			CheckCardinals(action, pos, 5, size);

			action(pos + new Point16(4, 3), size);
			action(pos + new Point16(3, 4), size);
			action(pos + new Point16(-3, 4), size);
			action(pos + new Point16(-4, 3), size);
			action(pos + new Point16(-4, -3), size);
			action(pos + new Point16(-3, -4), size);
			action(pos + new Point16(3, -4), size);
			action(pos + new Point16(4, -3), size);
		}

		private void Check12Rad6(Action<Point16, int> action, Point16 pos, int size)
		{
			CheckCardinals(action, pos, 6, size);

			action(pos + new Point16(5, 3), size);
			action(pos + new Point16(3, 5), size);
			action(pos + new Point16(-3, 5), size);
			action(pos + new Point16(-5, 3), size);
			action(pos + new Point16(-5, -3), size);
			action(pos + new Point16(-3, -5), size);
			action(pos + new Point16(3, -5), size);
			action(pos + new Point16(5, -3), size);
		}

		private void TryEngage(Point16 pos, int size)
		{
			var tile = Framing.GetTileSafely(pos);
			if (tile.type == ModContent.TileType<GearTile>())
			{
				if (DummyTile.DummyExists(pos.X, pos.Y, ModContent.ProjectileType<GearTileDummy>()))
				{
					var gearDummy = (GearTileDummy)DummyTile.GetDummy(pos.X, pos.Y, ModContent.ProjectileType<GearTileDummy>()).modProjectile;

					if (gearDummy.size == size && !gearDummy.engaged)
					{
						int thisSize = Teeth;
						int nextSize = gearDummy.Teeth;

						gearDummy.direction = direction * -1 * (thisSize / (float)nextSize);
						gearDummy.RecurseOverGears(gearDummy.TryEngage);
					}
				}
			}

			engaged = true;
		}

		private void TryDisengage(Point16 pos, int size)
		{
			var tile = Framing.GetTileSafely(pos);
			if (tile.type == ModContent.TileType<GearTile>())
			{
				if (DummyTile.DummyExists(pos.X, pos.Y, ModContent.ProjectileType<GearTileDummy>()))
				{
					var gearDummy = (GearTileDummy)DummyTile.GetDummy(pos.X, pos.Y, ModContent.ProjectileType<GearTileDummy>()).modProjectile;

					if (gearDummy.size == size && gearDummy.engaged)
					{
						gearDummy.direction = 0;
						gearDummy.RecurseOverGears(gearDummy.TryDisengage);
					}
				}
			}

			engaged = false;
		}

		public void Toggle()
		{
			if (engaged)
			{
				//engaged = false;
				TryDisengage(new Point16(ParentX, ParentY), Size);
			}
			else
			{
				direction = 2;
				TryEngage(new Point16(ParentX, ParentY), Size);
			}
		}
	}

	class GearTilePlacer : QuickTileItem
	{
		public GearTilePlacer() : base("Gear puzzle", "Debug item", ModContent.TileType<GearTile>(), 8, AssetDirectory.Debug, true) { }
	}
}
