using NetEasy;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.DummyTileSystem
{
	public abstract class Dummy : Entity, ILoadable, IPostLoadable
	{
		/// <summary>
		/// Numeric ID of this dummy. This is only consistent at runtime and depends on load order.
		/// </summary>
		public int type;

		public bool netUpdate;
		public int identity;

		public int offscreenRadius = 256;
		public bool offscreen;

		protected int validType;

		public Tile Parent => Main.tile[ParentX, ParentY];

		public virtual int ParentX => (int)Center.X / 16;
		public virtual int ParentY => (int)Center.Y / 16;

		public Dummy() { }

		public Dummy(int validType, int width, int height)
		{
			this.validType = validType;
			this.width = width;
			this.height = height;
		}

		public virtual bool ValidTile(Tile tile)
		{
			return tile.TileType == validType && tile.HasTile;
		}

		public void Load(Mod mod)
		{
			OnLoad(mod);
		}

		public void PostLoad()
		{
			// Build and register the prototype
			type = DummySystem.prototypes.Count;

			SafeSetDefaults();

			DummySystem.prototypes.Add(type, this);
			DummySystem.types.Add(GetType(), type);
		}

		public void PostLoadUnload()
		{
			Unload();
		}

		/// <summary>
		/// Additional effects that should run when this Dummy's prototype loads at load time. ``this`` will be the prototype here.
		/// </summary>
		/// <param name="mod">The Starlight River instance</param>
		public virtual void OnLoad(Mod mod) { }

		/// <summary>
		/// Unload anything that needs to be unloaded here
		/// </summary>
		public virtual void Unload() { }

		/// <summary>
		/// The equivelent of this dummy's AI. Runs every frame after projectiles update.
		/// </summary>
		public virtual void Update() { }

		/// <summary>
		/// Effects that should occur when this dummy collides with the player. Override Colliding to change the detection logic.
		/// </summary>
		/// <param name="Player">The player colliding with the dummy</param>
		public virtual void Collision(Player Player) { }

		/// <summary>
		/// Occurs whenever a new dummy instance is spawned. Similar to SetDefaults on various ModTypes.
		/// </summary>
		public virtual void SafeSetDefaults() { }

		/// <summary>
		/// Effects that occur when a new dummy instance is spawned after defaults are set.
		/// </summary>
		public virtual void OnSpawn() { }

		/// <summary>
		/// Sends extra net data when netUpdate = true
		/// </summary>
		/// <param name="writer"></param>
		public virtual void SafeSendExtraAI(BinaryWriter writer) { }

		/// <summary>
		/// Companion hook to SafeSendExtraAI, retrieve your data here.
		/// </summary>
		/// <param name="reader"></param>
		public virtual void SafeReceiveExtraAI(BinaryReader reader) { }

		/// <summary>
		/// Draws after all projectiles draw.
		/// </summary>
		/// <param name="lightColor"></param>
		public virtual void PostDraw(Color lightColor) { }

		/// <summary>
		/// Draws behind Tiles and NPCs. Equivelent to Projectile.Hide = true and DrawBehindNPCsAndTiles in DrawBehind hook.
		/// </summary>
		public virtual void DrawBehindTiles() { }

		/// <summary>
		/// Determines if a player should be considered colliding with this dummy or not.
		/// </summary>
		/// <param name="player">The player to check</param>
		/// <returns>If Collision should run or not</returns>
		public virtual bool Colliding(Player player)
		{
			return player.Hitbox.Intersects(Hitbox);
		}

		/// <summary>
		/// Determines how this entity is cloned (including cloning from the prototype on spawning)
		/// </summary>
		/// <returns>A clone of this dummy</returns>
		public virtual Dummy Clone()
		{
			return MemberwiseClone() as Dummy;
		}

		public void SendExtraAI(BinaryWriter writer)
		{
			// These three get ready earlier on to identify the dummy
			writer.Write(position.X);
			writer.Write(position.Y);
			writer.Write(type);

			writer.Write(validType);
			writer.Write(width);
			writer.Write(height);
			writer.Write(identity);

			SafeSendExtraAI(writer);
		}

		public void ReceiveExtraAI(BinaryReader reader)
		{
			validType = reader.ReadInt32();
			width = reader.ReadInt32();
			height = reader.ReadInt32();
			identity = reader.ReadInt32();

			var key = new Point16(ParentX, ParentY);
			DummyTile.dummiesByPosition[key] = this;

			SafeReceiveExtraAI(reader);
		}

		public void AI()
		{
			//multiplayer clients aren't allowed to kill dummies since they can have unloaded tiles
			if (!ValidTile(Parent) && active && Main.netMode != NetmodeID.MultiplayerClient)
			{
				DeleteDummyPacket deletePacket = new DeleteDummyPacket(position.X, position.Y, type);
				deletePacket.Send(runLocally: true);
				return;
			}

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];

				if (Colliding(player))
					Collision(player);
			}

			Update();

			Rectangle cullBox = Hitbox;
			cullBox.Inflate(offscreenRadius, offscreenRadius);
			offscreen = !cullBox.Intersects(new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight));

			if (netUpdate && Main.netMode == NetmodeID.Server)
			{
				var stream = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(stream);
				SendExtraAI(writer);
				new DummyPacket(stream.ToArray()).Send(-1, -1, false);

				writer.Dispose();
			}
		}

		public bool CanHitPlayer(Player target)
		{
			Collision(target);
			return false;
		}
	}

	[Serializable]
	public class DummyPacket : Module
	{
		public byte[] data;

		public DummyPacket(byte[] data)
		{
			this.data = data;
		}

		protected override void Receive()
		{
			MemoryStream stream = new(data);
			BinaryReader reader = new(stream);

			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			int type = reader.ReadInt32();

			Dummy dummy = DummyTile.GetDummy((int)(x / 16), (int)(y / 16), type);

			if (dummy != null)
			{
				dummy.position = new Vector2(x, y);
				dummy.type = type;

				dummy.ReceiveExtraAI(reader);
			} else
			{
				// this case means a client is receiving an update for a dummy that did not exist before 

				Vector2 spawnPos = new Vector2(x, y) + DummySystem.prototypes[type].Size / 2;
				Dummy newDummy = DummySystem.NewDummy(type, spawnPos);

				newDummy.position = new Vector2(x, y);
				newDummy.type = type;

				newDummy.ReceiveExtraAI(reader);

				var key = new Point16((int)(x / 16), (int)(y / 16));
				DummyTile.dummiesByPosition[key] = newDummy;
			}

			reader.Dispose();
		}
	}

	/// <summary>
	/// Multiplayer clients aren't allowed to kill dummies themselves, so the server will tell them when to delete the dummy
	/// </summary>
	[Serializable]
	public class DeleteDummyPacket : Module
	{
		public readonly float x;
		public readonly float y;
		public readonly int type;

		public DeleteDummyPacket(float x, float y, int type)
		{
			this.x = x;
			this.y = y;
			this.type = type;
		}

		protected override void Receive()
		{
			Dummy dummy = DummyTile.GetDummy((int)(x / 16), (int)(y / 16), type);

			if (dummy != null)
				dummy.active = false;
		}
	}
}