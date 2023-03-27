using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	public abstract class GearTile : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<GearTileDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<GearTileEntity>().Hook_AfterPlacement, -1, 0, false);
			this.QuickSetFurniture(1, 1, 1, SoundID.PlayerHit, new Color(1, 1, 1)); // Is the sound correct..?
		}

		public virtual void OnEngage(GearTileEntity entity) { }

		public virtual void OnDisengage(GearTileEntity entity) { }
	}

	public class GearTileEntity : ModTileEntity
	{
		public bool engaged = false;
		public int size;
		public float rotationVelocity;
		public float rotationOffset;

		public int Teeth => size switch
		{
			0 => 1,
			1 => 4,
			2 => 8,
			3 => 12,
			_ => 1,
		};

		public override bool IsTileValidForEntity(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return ModContent.GetModTile(tile.TileType) is GearTile && tile.HasTile;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
				return -1;
			}

			return Place(i, j);
		}

		public override void Update()
		{
			if (!IsTileValidForEntity(Position.X, Position.Y))
				Kill(Position.X, Position.Y);
		}

		/// <summary>
		/// Performs an action on all gears in a system. Has no built-in base case, you must implement one in your action.
		/// </summary>
		/// <param name="action">The action to be performed on all connected gears, including this one</param>
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

		/// <summary>
		/// Disengages the connected system and then restarts it from this gear, with the given initial speed
		/// </summary>
		/// <param name="rotationVelocity">The speed of this gear in the new engagement</param>
		public void Engage(float rotationVelocity)
		{
			Disengage();
			this.rotationVelocity = rotationVelocity;
			Engage(Position, size);
		}

		private void Engage(Point16 pos, int size)
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
					float ratio = thisSize / (float)nextSize;

					entity.rotationVelocity = rotationVelocity * -1 * ratio;

					if (entity == this) //This is here to prevent the first gear which engages from reversing itself
						entity.rotationVelocity *= -1;

					float trueAngle = (Position.ToVector2() * 16 + Vector2.One * 8 - (entity.Position.ToVector2() * 16 + Vector2.One * 8)).ToRotation();

					entity.rotationOffset = -(ratio * rotationOffset) + (1 + ratio) * trueAngle + (float)Math.PI / entity.Teeth;

					engaged = true;

					entity.RecurseOverGears(entity.Engage);
				}

				Tile tile = Main.tile[Position.X, Position.Y];
				(ModContent.GetModTile(tile.TileType) as GearTile)?.OnEngage(this);
			}

			engaged = true;
		}

		/// <summary>
		/// Disengage the gear system connected to this gear
		/// </summary>
		public void Disengage()
		{
			Disengage(Position, size);
		}

		private void Disengage(Point16 pos, int size)
		{
			if (!ByPosition.ContainsKey(pos))
				return;

			var entity = ByPosition[pos] as GearTileEntity;

			if (entity != null)
			{
				if (entity.size == size && entity.engaged)
				{
					entity.rotationVelocity = 0;
					entity.rotationOffset = 0;

					engaged = false;

					Tile tile = Main.tile[Position.X, Position.Y];
					(ModContent.GetModTile(tile.TileType) as GearTile)?.OnDisengage(this);

					entity.RecurseOverGears(entity.Disengage);
				}
			}

			engaged = false;
		}

		/// <summary>
		/// Toggles the connected gear system between being on and off
		/// </summary>
		/// <param name="rotationVelocity">The speed of this gear in the new system if toggling on</param>
		public void Toggle(float rotationVelocity)
		{
			if (engaged)
			{
				Disengage(Position, size);
			}
			else
			{
				this.rotationVelocity = rotationVelocity;
				Engage(Position, size);
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(engaged);
			writer.Write(size);
			writer.Write(rotationVelocity);
			writer.Write(rotationOffset);
		}

		public override void NetReceive(BinaryReader reader)
		{
			engaged = reader.ReadBoolean();
			size = reader.ReadInt32();
			rotationVelocity = reader.ReadSingle();
			rotationOffset = reader.ReadSingle();
		}

		public override void SaveData(TagCompound tag)
		{
			tag["engaged"] = engaged;
			tag["size"] = size;
			tag["direction"] = rotationVelocity;
			tag["rotationOffset"] = rotationOffset;
		}

		public override void LoadData(TagCompound tag)
		{
			engaged = tag.GetBool("engaged");
			size = tag.GetInt("size");
			rotationVelocity = tag.GetFloat("direction");
			rotationOffset = tag.GetFloat("rotationOffset");
		}
	}

	public abstract class GearTileDummy : Dummy
	{
		public int gearAnimation;
		public int oldSize;

		protected bool Engaged
		{
			get => GearEntity.engaged;
			set => GearEntity.engaged = value;
		}

		protected float RotationVelocity
		{
			get => GearEntity.rotationVelocity;
			set => GearEntity.rotationVelocity = value;
		}

		protected float RotationOffset
		{
			get => GearEntity.rotationOffset;
			set => GearEntity.rotationOffset = value;
		}

		protected GearTileEntity GearEntity => TileEntity.ByPosition[new Point16(ParentX, ParentY)] as GearTileEntity;

		public int Size
		{
			get => GearEntity.size;
			set => GearEntity.size = value % 4;
		}

		public float Rotation
		{
			get
			{
				float rot = 0;

				if (Engaged)
					rot = Main.GameUpdateCount * 0.01f * RotationVelocity;

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

			if (gearAnimation == 15 && Size != 0)
			{
				for (int k = 0; k < 10 * Size; k++)
				{
					Vector2 off = Vector2.One.RotatedByRandom(6.28f);
					Dust.NewDustPerfect(Projectile.Center + off * Size * 10, ModContent.DustType<Dusts.GlowFastDecelerate>(), off * Main.rand.NextFloat(Size * 2 - 2, Size * 2) * 0.6f, 0, new Color(100, 200, 255), 0.5f);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Size switch
			{
				0 => ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value,
				1 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value,
				2 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearMid").Value,
				3 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearLarge").Value,
				_ => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value,
			};
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);
		}
	}
}