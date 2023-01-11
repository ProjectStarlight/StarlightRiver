//TODO on salt:
//Make it generate
//Make it so you can place tiles on top of it

using StarlightRiver.Content.Items.Food;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Cooking
{
	class TableSalt : ModTile
	{
		public virtual int DustType => DustID.Marble;

		public virtual int ProjectileType => ModContent.ProjectileType<TableSaltProjectile>();

		public virtual int ItemDrop => ModContent.ItemType<Items.Food.TableSalt>();

		public override string Texture => AssetDirectory.CookingTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileMerge[Type][TileID.Sand] = true;
			Main.tileMerge[TileID.Sand][Type] = true;
			this.QuickSet(0, DustType, SoundID.Dig, Color.White, ItemDrop);
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileBlockLight[Type] = false;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			if (WorldGen.noTileActions)
				return true;

			Tile above = Main.tile[i, j - 1];
			Tile below = Main.tile[i, j + 1];
			bool canFall = true;

			if (below == null || below.HasTile)
				canFall = false;

			if (above.HasTile && (TileID.Sets.BasicChest[above.TileType] || TileID.Sets.BasicChestFake[above.TileType] || above.TileType == TileID.PalmTree || TileID.Sets.BasicDresser[above.TileType]))
				canFall = false;

			if (canFall)
			{
				float positionX = i * 16 + 8;
				float positionY = j * 16 + 8;

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.tile[i, j].ClearTile();
					int proj = Projectile.NewProjectile(null, positionX, positionY, 0f, 0.41f, ProjectileType, 10, 0f, Main.myPlayer);
					Main.projectile[proj].ai[0] = 1f;
					WorldGen.SquareTileFrame(i, j);
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					Main.tile[i, j].ClearTile();
					bool spawnProj = true;

					for (int k = 0; k < 1000; k++)
					{
						Projectile otherProj = Main.projectile[k];

						if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == ProjectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f)
						{
							spawnProj = false;
							break;
						}
					}

					if (spawnProj)
					{
						int proj = Projectile.NewProjectile(null, positionX, positionY, 0f, 2.5f, ProjectileType, 10, 0f, Main.myPlayer);
						Main.projectile[proj].velocity.Y = 0.5f;
						Main.projectile[proj].position.Y += 2f;
						Main.projectile[proj].netUpdate = true;
					}

					NetMessage.SendTileSquare(-1, i, j, 1);
					WorldGen.SquareTileFrame(i, j);
				}
				return false;
			}
			return true;
		}
	}

	class TableSaltProjectile : ModProjectile
	{
		protected bool falling = true;

		protected virtual int TileType => ModContent.TileType<TableSalt>();

		public virtual int DustType => DustID.Marble;

		public override string Texture => AssetDirectory.CookingTile + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sea Salt");
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
			Projectile.localAI[1] = 0f;

			if (Projectile.ai[0] == 1f)
			{
				if (!falling)
				{
					Projectile.ai[1] += 1f;

					if (Projectile.ai[1] >= 60f)
					{
						Projectile.ai[1] = 60f;
						Projectile.velocity.Y += 0.2f;
					}
				}
				else
					Projectile.velocity.Y += 0.41f;
			}
			else if (Projectile.ai[0] == 2f)
			{
				Projectile.velocity.Y += 0.2f;

				if (Projectile.velocity.X < -0.04f)
					Projectile.velocity.X += 0.04f;
				else if (Projectile.velocity.X > 0.04f)
					Projectile.velocity.X -= 0.04f;
				else
					Projectile.velocity.X = 0f;
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

		public override bool? CanDamage() => Projectile.localAI[1] != -1f;
	}

	class PinkSeaSalt : TableSalt
	{
		public override int DustType => DustID.Orichalcum;


		public override int ProjectileType => ModContent.ProjectileType<PinkSeaSaltProjectile>();

		public override int ItemDrop => ModContent.ItemType<Items.Food.SeaSalt>();
	}

	class PinkSeaSaltProjectile : TableSaltProjectile
	{
		protected override int TileType => ModContent.TileType<PinkSeaSalt>();

		public override int DustType => DustID.Orichalcum;
	}
}
