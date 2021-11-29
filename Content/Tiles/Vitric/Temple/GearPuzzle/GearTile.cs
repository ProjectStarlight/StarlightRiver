using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Vitric
{
	public abstract class GearTile : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<GearTileDummy>();

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.Invisible;
			return base.Autoload(ref name, ref texture);
		}

		public override void SetDefaults()
		{
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<GearTileEntity>().Hook_AfterPlacement, -1, 0, false);
			QuickBlock.QuickSetFurniture(this, 1, 1, 1, 1, new Color(1, 1, 1));
		}

        public override bool NewRightClick(int i, int j)
		{
			var dummy = (Dummy(i, j).modProjectile as GearTileDummy);

			var entity = TileEntity.ByPosition[new Point16(i, j)] as GearTileEntity;

			if (entity is null)
				return false;

			if (dummy is null || dummy.gearAnimation > 0)
				return false;

			if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.DebugStick>())
			{
				entity.Toggle();			
				return true;
			}

			dummy.oldSize = dummy.Size;
			dummy.Size++;
			dummy.gearAnimation = 40;

			return true;
		}

		public virtual void OnEngage(GearTileEntity entity) { }

		public virtual void OnDisengage(GearTileEntity entity) { }
	}

	public class GearTileEntity : ModTileEntity
	{
		public bool engaged = false;
		public int size;
		public float direction;
		public float rotationOffset;

		public int Teeth
		{
			get
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
		}

		public override bool ValidTile(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return ModContent.GetModTile(tile.type) is GearTile && tile.active();
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
				return -1;
			}

			return Place(i, j);
		}

		public void RecurseOverGears(Action<Point16, int> action)
		{
			if (size > 0)
			{
				Point16 pos = Position;

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
			if (!ByPosition.ContainsKey(pos))
				return;

			var entity = ByPosition[pos] as GearTileEntity;

			if (entity != null)
			{
				if (entity.size == size && !entity.engaged)
				{
					int thisSize = Teeth;
					int nextSize = entity.Teeth;
					float ratio = (thisSize / (float)nextSize);

					entity.direction = direction * -1 * ratio;

					float trueAngle = ((Position.ToVector2() * 16 + Vector2.One * 8) - (entity.Position.ToVector2() * 16 + Vector2.One * 8)).ToRotation();

					entity.rotationOffset = -(ratio * rotationOffset) + ((1 + ratio) * trueAngle) + (float)Math.PI / entity.Teeth;

					//gearDummy.rotationOffset += rotationOffset;
					engaged = true;
					entity.RecurseOverGears(entity.TryEngage);
				}
			}

			engaged = true;

			Tile tile = Main.tile[Position.X, Position.Y];
			(ModContent.GetModTile(tile.type) as GearTile)?.OnEngage(this);		
		}

		private void TryDisengage(Point16 pos, int size)
		{
			if (!ByPosition.ContainsKey(pos))
				return;

			var entity = ByPosition[pos] as GearTileEntity;

			if (entity != null)
			{
				if (entity.size == size && entity.engaged)
				{
					entity.direction = 0;
					entity.rotationOffset = 0;

					engaged = false;
					entity.RecurseOverGears(entity.TryDisengage);
				}
			}

			engaged = false;

			Tile tile = Main.tile[Position.X, Position.Y];
			(ModContent.GetModTile(tile.type) as GearTile)?.OnDisengage(this);		
		}

		public void Toggle()
		{
			if (engaged)
			{
				//engaged = false;
				TryDisengage(Position, size);
			}
			else
			{
				direction = 2;
				TryEngage(Position, size);
			}
		}

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			writer.Write(engaged);
			writer.Write(size);
			writer.Write(direction);
			writer.Write(rotationOffset);
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive)
		{
			engaged = reader.ReadBoolean();
			size = reader.ReadInt32();
			direction = reader.ReadSingle();
			rotationOffset = reader.ReadSingle();
		}

		public override TagCompound Save()
		{
			return new TagCompound()
			{
				["engaged"] = engaged,
				["size"] = size,
				["direction"] = direction,
				["rotationOffset"] = rotationOffset
			};
		}

		public override void Load(TagCompound tag)
		{
			engaged = tag.GetBool("engaged");
			size = tag.GetInt("size");
			direction = tag.GetFloat("direction");
			rotationOffset = tag.GetFloat("rotationOffset");
		}
	}

	public abstract class GearTileDummy : Dummy
	{
		public int gearAnimation;
		public int oldSize;

		bool Engaged
		{
			get => Entity.engaged;
			set => Entity.engaged = value;
		}

		float Direction
		{
			get => Entity.direction;
			set => Entity.direction = value;
		}

		protected float RotationOffset
		{
			get => Entity.rotationOffset;
			set => Entity.rotationOffset = value;
		}

		private GearTileEntity Entity => TileEntity.ByPosition[new Point16(ParentX, ParentY)] as GearTileEntity;

		public int Size
		{
			get => Entity.size;
			set => Entity.size = value % 4;
		}

		public float Rotation
		{
			get
			{
				float rot = 0;

				if (Engaged)
					rot = Main.GameUpdateCount * 0.01f * Direction;

				return rot + RotationOffset;
			}
		}

		public GearTileDummy(int type) : base(type, 16, 16) { }

		public override void Update()
		{
			if (gearAnimation > 0)
				gearAnimation--;

			if (oldSize == 0 && gearAnimation > 20) //no fadeout when there is nothing to fade out
				gearAnimation = 20;

			if(gearAnimation == 15 && Size != 0)
			{
				for (int k = 0; k < 10 * Size; k++)
				{
					Vector2 off = Vector2.One.RotatedByRandom(6.28f);
					Dust.NewDustPerfect(projectile.Center + off * Size * 10, ModContent.DustType<Dusts.GlowFastDecelerate>(), off * Main.rand.NextFloat(Size * 2 - 2, Size * 2) * 0.6f, 0, new Color(100, 200, 255), 0.5f);
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D tex;

			switch (Size)
			{
				case 0: tex = ModContent.GetTexture(AssetDirectory.Invisible); break;
				case 1: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
				case 2: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearMid"); break;
				case 3: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearLarge"); break;
				default: tex = ModContent.GetTexture(AssetDirectory.VitricTile + "MagicalGearSmall"); break;
			}

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}
}
