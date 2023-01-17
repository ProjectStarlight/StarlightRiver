using StarlightRiver.Content.Tiles.BaseTypes;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Cooking
{
	internal class TableSalt : ModTile
	{
		public new virtual int DustType => DustID.Marble;

		public virtual int ProjectileType => ModContent.ProjectileType<TableSaltProjectile>();

		public new virtual int ItemDrop => ModContent.ItemType<Items.Food.TableSalt>();

		public virtual int TileType => ModContent.TileType<TableSalt>();

		public override string Texture => AssetDirectory.CookingTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileMerge[TileID.Sand][Type] = true;
			this.QuickSet(0, DustType, SoundID.Dig, Color.White, ItemDrop);
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileBlockLight[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.addTile(Type);
		}

		public override bool CanPlace(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j + 1);
			if (tile.HasTile && (tile.TileType == TileType || Main.tileSolid[tile.TileType]))
			{
				Main.tileSolid[tile.TileType] = true;
				return true;
			}

			Tile tile2 = Framing.GetTileSafely(i, j - 1);
			if (tile2.HasTile && (tile2.TileType == TileType || Main.tileSolid[tile2.TileType]))
			{
				Main.tileSolid[tile2.TileType] = true;
				return true;
			}

			Tile tile3 = Framing.GetTileSafely(i + 1, j);
			if (tile3.HasTile && (tile3.TileType == TileType || Main.tileSolid[tile3.TileType]))
			{
				Main.tileSolid[tile3.TileType] = true;
				return true;
			}

			Tile tile4 = Framing.GetTileSafely(i - 1, j);
			if (tile4.HasTile && (tile4.TileType == TileType || Main.tileSolid[tile.TileType]))
			{
				Main.tileSolid[tile4.TileType] = true;
				return true;
			}

			return false;
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Main.tileSolid[tile.TileType] = false;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.tile[i, j].LiquidAmount > 0)
				WorldGen.KillTile(i, j, default, default, true);
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

	internal class PinkSeaSalt : TableSalt
	{
		public override int DustType => DustID.Orichalcum;

		public override int TileType => ModContent.TileType<PinkSeaSalt>();

		public override int ProjectileType => ModContent.ProjectileType<PinkSeaSaltProjectile>();

		public override int ItemDrop => ModContent.ItemType<Items.Food.SeaSalt>();
	}

	internal class PinkSeaSaltProjectile : FallingTileProjectile
	{
		protected override int TileType => ModContent.TileType<PinkSeaSalt>();

		protected override int DustType => DustID.Orichalcum;

		protected override string ProjectileName => "Sea Salt";

		public override string Texture => AssetDirectory.CookingTile + Name;
	}

	internal class TableSaltProjectile : FallingTileProjectile
	{
		protected override int TileType => ModContent.TileType<TableSalt>();

		protected override int DustType => DustID.Marble;

		protected override string ProjectileName => "Table Salt";

		public override string Texture => AssetDirectory.CookingTile + Name;
	}
}
