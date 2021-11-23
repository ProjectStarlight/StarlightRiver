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
	public abstract class GearTile : DummyTile
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

        public override bool SpawnConditions(int i, int j)
        {
			Tile tile = Main.tile[i, j];
			return tile.frameX < 4 && tile.frameY == 0;
		}

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
			var tile = Framing.GetTileSafely(i, j);
			var dummy = (Dummy(i, j).modProjectile as GearTileDummy);

			if(dummy != null)
				dummy.Size = tile.frameX;
        }

        public override bool NewRightClick(int i, int j)
		{
			var dummy = (Dummy(i, j).modProjectile as GearTileDummy);

			if (dummy.gearAnimation > 0)
				return false;

			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				dummy.Toggle();			
				return true;
			}

			dummy.oldSize = dummy.Size;
			dummy.Size++;
			dummy.gearAnimation = 40;

			return true;
		}
	}

	public abstract class GearTileDummy : Dummy
	{
		public int gearAnimation;
		public int oldSize;

		bool engaged = false;
		float direction = 0;

		protected int size;
		protected float rotationOffset;

		public int Size
		{
			get => size;
			set
            {
				size = value % 4;
				Parent.frameX = (short)size;
            }
		}

		public float Rotation
		{
			get
			{
				float rot = 0;

				if (engaged)
					rot = Main.GameUpdateCount * 0.01f * direction;

				if (direction > 0)
					rot += (float)Math.PI / Teeth;

				return rot + rotationOffset;
			}
		}

		public int Teeth => GetTeeth();

		public GearTileDummy(int type) : base(type, 16, 16) { }

        public int GetTeeth()
		{
			switch (size)
			{
				case 0: return 1;
				case 1: return 4;
				case 2: return 8;
				case 3: return 12;
				default: return 1;
			}
		}

		public override void Update()
		{
			if (gearAnimation > 0)
				gearAnimation--;

			if (oldSize == 0 && gearAnimation > 20) //no fadeout when there is nothing to fade out
				gearAnimation = 20;

			if(gearAnimation == 15 && size != 0)
			{
				for (int k = 0; k < 10 * size; k++)
				{
					Vector2 off = Vector2.One.RotatedByRandom(6.28f);
					Dust.NewDustPerfect(projectile.Center + off * size * 10, ModContent.DustType<Dusts.GlowFastDecelerate>(), off * Main.rand.NextFloat(size * 2 - 2, size * 2) * 0.6f, 0, new Color(100, 200, 255), 0.5f);
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D tex;

			switch (size)
			{
				case 0: tex = ModContent.GetTexture(AssetDirectory.Invisible); break;
				case 1: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
				case 2: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearMid"); break;
				case 3: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearLarge"); break;
				default: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
			}

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);
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
			if (DummyTile.DummyExists<GearTileDummy>(pos.X, pos.Y))
			{
				var gearDummy = (GearTileDummy)DummyTile.GetDummy<GearTileDummy>(pos.X, pos.Y).modProjectile;

				if (gearDummy.size == size && !gearDummy.engaged)
				{
					int thisSize = Teeth;
					int nextSize = gearDummy.Teeth;

					gearDummy.direction = direction * -1 * (thisSize / (float)nextSize);
					gearDummy.rotationOffset = (projectile.Center - gearDummy.projectile.Center).ToRotation();
					gearDummy.RecurseOverGears(gearDummy.TryEngage);
				}
			}

			OnEngage();
			engaged = true;
		}

		private void TryDisengage(Point16 pos, int size)
		{
			if (DummyTile.DummyExists<GearTileDummy>(pos.X, pos.Y))
			{
				var gearDummy = (GearTileDummy)DummyTile.GetDummy<GearTileDummy>(pos.X, pos.Y).modProjectile;

				if (gearDummy.size == size && gearDummy.engaged)
				{
					gearDummy.direction = 0;
					gearDummy.RecurseOverGears(gearDummy.TryDisengage);
				}
			}

			OnDisengage();
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

		public virtual void OnEngage() { }

		public virtual void OnDisengage() { }
	}
}
