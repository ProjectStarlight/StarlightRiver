using Terraria.ID;

namespace StarlightRiver.Content.Tiles.BaseTypes
{
	internal abstract class FallingTileProjectile : ModProjectile
	{
		protected bool falling = true;

		protected virtual int TileType => -1;

		protected virtual int DustType => -1;

		protected virtual string ProjectileName => "";

		protected ref float fallDelay => ref Projectile.ai[1];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(ProjectileName);
			ProjectileID.Sets.ForcePlateDetection[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.knockBack = 6f;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (Main.rand.NextBool(5))
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType);
				Main.dust[dust].velocity.X *= 0.4f;
			}

			Projectile.tileCollide = true;

			if (!falling)
			{
				fallDelay += 1f;

				if (fallDelay >= 60f)
				{
					fallDelay = 60f;
					Projectile.velocity.Y += 0.2f;
				}
			}
			else
			{
				Projectile.velocity.Y += 0.41f;
			}

			Projectile.rotation += 0.1f;

			if (Projectile.velocity.Y > 10f)
				Projectile.velocity.Y = 10f;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			if (falling)
				Projectile.velocity = Collision.AnyCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true);
			else
				Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, fallThrough, fallThrough, 1);

			return false;
		}

		public override void Kill(int timeLeft)
		{
			if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem)
			{
				int tileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
				int tileY = (int)(Projectile.position.Y + Projectile.width / 2) / 16;

				Tile tile = Main.tile[tileX, tileY];
				Tile tileBelow = Main.tile[tileX, tileY + 1];

				if (tile.BlockType == BlockType.HalfBlock && Projectile.velocity.Y > 0f && System.Math.Abs(Projectile.velocity.Y) > System.Math.Abs(Projectile.velocity.X))
					tileY--;

				if (!tile.HasTile)
				{
					bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.HasTile && tileBelow.TileType == TileID.MinecartTrack;

					if (!onMinecartTrack)
						WorldGen.PlaceTile(tileX, tileY, TileType, false, true);

					if (!onMinecartTrack && tile.HasTile && tile.TileType == TileType)
					{
						if (tileBelow.BlockType == BlockType.HalfBlock || tileBelow.Slope != 0)
						{
							WorldGen.SlopeTile(tileX, tileY + 1, 0);

							if (Main.netMode == NetmodeID.Server)
								NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, tileX, tileY + 1);
						}

						if (Main.netMode != NetmodeID.SinglePlayer)
							NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, tileX, tileY, TileType);
					}
				}
			}
		}

		public override bool? CanDamage()
		{
			return true;
		}
	}
}
