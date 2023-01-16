using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.DummyTileSystem
{
	public abstract class Dummy : ModProjectile
	{
		protected int validType;
		private int width;
		private int height;

		public Tile Parent => Main.tile[ParentX, ParentY];

		public virtual int ParentX => (int)Projectile.Center.X / 16;
		public virtual int ParentY => (int)Projectile.Center.Y / 16;

		public override string Texture => AssetDirectory.Invisible;

		public Dummy(int validType, int width, int height)
		{
			this.validType = validType;
			this.width = width;
			this.height = height;
		}

		public virtual bool ValidTile(Tile tile)
		{
			return tile.TileType == validType && tile.HasTile; //the tile is null only where tiles are unloaded in multiPlayer. We don't want to kill off dummies on unloaded tiles until tile is known because Projectile is recieved MUCH farther than the tiles.
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public virtual void Update() { }

		public virtual void Collision(Player Player) { }

		public virtual void SafeSetDefaults() { }

		public virtual void SafeSendExtraAI(BinaryWriter writer) { }

		public virtual void SafeReceiveExtraAI(BinaryReader reader) { }

		public sealed override void SetStaticDefaults()
		{
			DisplayName.SetDefault("");
		}

		public sealed override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(validType);
			writer.Write(width);
			writer.Write(height);

			SafeSendExtraAI(writer);
		}

		public sealed override void ReceiveExtraAI(BinaryReader reader)
		{
			validType = reader.ReadInt32();
			width = reader.ReadInt32();
			height = reader.ReadInt32();

			var key = new Point16(ParentX, ParentY);
			DummyTile.dummies[key] = Projectile;

			SafeReceiveExtraAI(reader);
		}

		public sealed override void SetDefaults()
		{
			SafeSetDefaults();

			Projectile.width = width;
			Projectile.height = height;
			Projectile.hostile = true;
			Projectile.damage = 1;
			Projectile.timeLeft = 2;
			Projectile.netImportant = true;
		}

		public sealed override void AI()
		{
			if (ValidTile(Parent))
				Projectile.timeLeft = 2;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				//in single Player we can use the CanHitPlayer, but in MP that is only run by the server so we need to check Players manually for clients
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player Player = Main.player[i];

					if (Player.Hitbox.Intersects(Projectile.Hitbox))
						Collision(Player);
				}
			}

			Update();
		}

		public sealed override bool CanHitPlayer(Player target)
		{
			Collision(target);
			return false;
		}
	}
}
