using StarlightRiver.Core.Systems.AuroraWaterSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Content.Dusts.ArtifactSparkles;

namespace StarlightRiver.Content.Items.Misc
{
	public class EnchantedWateringCan : ModItem
	{//would also fit in forest namespace, but since its found underground its in misc instead
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Watering Can");
			Tooltip.SetDefault("Grows saplings into large saplings\nSpeeds up the growth of large saplings");
		}

		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 38;

			Item.useTime = 12;
			Item.useAnimation = 12;

			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<EnchantedWateringCanProj>();
			Item.shootSpeed = 14f;

			Item.autoReuse = false;
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = ItemRarityID.Blue;

			Item.autoReuse = true;
			Item.reuseDelay = 70;
			Item.mana = 20;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.IronBar, 30)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	internal class EnchantedWateringCanProj : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + "EnchantedWateringCanProj";

		Player Owner => Main.player[Projectile.owner];	

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Watering Can");
			//Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(104, 38);
			//Projectile.penetrate = -1;
			//Projectile.ownerHitCheck = true;
			//Projectile.extraUpdates = 3;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.spriteDirection = Owner.direction;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Owner.heldProj = Projectile.whoAmI;

			Vector2 armOffset = Main.GetPlayerArmPosition(Projectile) - Owner.Center;
			Projectile.Center = Owner.Center + armOffset * new Vector2(4.50f, 0.75f);

			Projectile.rotation = (float)Math.Sin(Projectile.ai[0] * 0.175f * Projectile.spriteDirection * 0.5f) * 0.3f;
			//version that points the upper angle upwards instead: //((direction.Y - 6) * 0.05f * -Projectile.spriteDirection);

			//matches the dust to the sprite
			Dust.NewDustPerfect(
				Projectile.Center +
					new Vector2(Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4)) + //random offset
					new Vector2(8 * Projectile.spriteDirection, -6) + //offset to center
					new Vector2(14 * Projectile.spriteDirection, 0).RotatedBy(
						Projectile.rotation * 2 + 0.4f * Projectile.spriteDirection),
				/*DustID.BlueFairy,*/ DustID.Water, 
				/*Vector2.Zero, default, default, 0.3f);*/ new Vector2(Main.rand.NextFloat(0f, 0.75f) * Projectile.spriteDirection, Main.rand.NextFloat(-0.2f, 0.2f)));//velocity

			Projectile.ai[0]++;

			const int ProjectileTime = 56;

			if (Projectile.ai[0] > ProjectileTime)
				Projectile.active = false;

			if(Projectile.ai[0] == ProjectileTime / 2)//check for saplings halfway through item use time
			{
				int hitboxTileWidth = Projectile.width / 16;
				int hitboxTileHeight = Projectile.height / 16;
				for (int s = 0; s < hitboxTileWidth; s++)//x
				{
					for (int t = 0; t < hitboxTileHeight; t++)//y
					{
						Vector2 pos = Projectile.position / 16 + new Vector2(0.5f, 0.66f);
						//SpawnDustAtTile((int)pos.X + s, (int)pos.Y + t);//debug
						if (CheckForSapling((int)pos.X + s, (int)pos.Y + t))
							break;
						else if(CheckForLargeSapling((int)pos.X + s, (int)pos.Y + t))
						{
							s++;//skip next x value
							break;
						}
					}
				}
			}
		}

		//void SpawnDustAtTile(int i, int j, int DustID = DustID.BlueFairy)
		//{
		//	for (int g = 0; g < 16; g++)
		//	{
		//		Dust.NewDustPerfect(
		//		new Vector2(i, j) * 16 + new Vector2(Main.rand.Next(0, 16), Main.rand.Next(0, 16)),
		//		DustID,
		//		Vector2.Zero, default, default, 0.35f);
		//	}
		//}

		public bool CheckForSapling(int i, int j)
		{
			if (!(Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Saplings))//doesn't work on vanity saplings
				return false;

			int offsetY = Main.tile[i, j].TileFrameY / 18;

			//check if tile sapling is on is a valid tile
			int belowTileType = Main.tile[i, j - offsetY + 2].TileType;
			if (!(belowTileType == TileID.Dirt || belowTileType == TileID.Grass || belowTileType == TileID.GolfGrass))
				return false;

			bool leftOpen = true;
			bool rightOpen = true;
			for (int s = 0; s < 3; s++)//x
			{
				if (s == 1)
					continue;
				for (int t = 0; t < 3; t++)//y
				{
					int posX = i - 1 + s;
					int poxY = j - offsetY + t;
					if (s == 0)
					{
						int type = Main.tile[posX, poxY].TileType;
						if (t == 2)
						{
							leftOpen = leftOpen && Main.tile[posX, poxY].HasTile && (type == TileID.Grass || type == TileID.Dirt || type == TileID.GolfGrass);
						}
						else
						{
							leftOpen = leftOpen && !(Main.tile[posX, poxY].HasTile &&
								(!Main.tileCut[type] ||
								type == TileID.Saplings ||
								type == TileID.VanityTreeSakuraSaplings ||
								type == TileID.VanityTreeWillowSaplings));//can break other saplings
						}

						//SpawnDustAtTile(i - 1 + s, j - offsetY + t, leftOpen ? DustID.GreenFairy : DustID.PinkFairy);//debug
					}

					if (s == 2)
					{
						int type = Main.tile[posX, poxY].TileType;
						if (t == 2)
						{
							rightOpen = rightOpen && Main.tile[posX, poxY].HasTile && (type == TileID.Grass || type == TileID.Dirt || type == TileID.GolfGrass);
						}
						else
						{
							rightOpen = rightOpen && !(Main.tile[posX, poxY].HasTile &&
								!(Main.tileCut[type] ||
								type == TileID.Saplings ||
								type == TileID.VanityTreeSakuraSaplings ||
								type == TileID.VanityTreeWillowSaplings));//can break other saplings
						}

						//SpawnDustAtTile(i - 1 + s, j - offsetY + t, rightOpen ? DustID.GreenFairy : DustID.PinkFairy);//debug
					}
				}
			}

			if(leftOpen || rightOpen)
			{
				int offsetX =
					(leftOpen && rightOpen) ? (Main.rand.NextBool() ? -1 : 0) :
					leftOpen ? -1 : 0;//last case it could be is right

				for (int s = 0; s < 2; s++)//x
				{
					for (int t = -1; t < 2; t++)//y
					{
						int posX = i + offsetX + s;
						int posY = j - offsetY + t;

						if (Main.tile[posX, posY].TileType == TileID.Saplings)//makes removing the sapling silent
							Main.tile[posX, posY].ClearTile();
						else
							WorldGen.KillTile(posX, posY);

						Gore.NewGore(
							new EntitySource_TileInteraction(Owner, posX, posY), 
							new Vector2(posX, posY) * 16 + new Vector2(Main.rand.Next(0, 16), Main.rand.Next(0, 16)), 
							Vector2.Zero, 
							GoreID.TreeLeaf_Normal);
					}
				}

				Helper.PlaceMultitile(new Point16(i + offsetX, j - offsetY - 1), ModContent.TileType<ThickTreeSapling>());
			}
			//Main.NewText(Main.tile[i, j].TileFrameX + " | " + Main.tile[i, j].TileFrameY);
			return true;
		}

		public bool CheckForLargeSapling(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileType == ModContent.TileType<ThickTreeSapling>())
			{
				int offsetX = tile.TileFrameX % 36 / 18;
				int offsetY = tile.TileFrameY / 18;
				//Main.NewText(Main.tile[i, j].TileFrameX + " | " + Main.tile[i, j].TileFrameY);
				int posX = i - offsetX;
				int posY = j - offsetY;

				for (int s = 0; s < 2; s++)//x
				{
					for (int t = 0; t < 3; t++)//y
					{
						if (!Main.rand.NextBool(2, 3))
							continue;

						int posX2 = i - offsetX + s;
						int posY2 = j - offsetY + t;

						Dust.NewDustPerfect(
							new Vector2(posX2, posY2) * 16 + new Vector2(Main.rand.Next(0, 17) + 4, Main.rand.Next(4, 21)),
							ModContent.DustType<LimeArtifactSparkle>(),
							new Vector2(Main.rand.NextFloat(-0.05f, 0.05f), Main.rand.NextFloat(-0.3f, -0.15f)),
							0,
							new Color(230, 255, 255), 1f);
					}
				}

				ModContent.GetModTile(Main.tile[posX, posY].TileType)?.RandomUpdate(posX, posY);
				NetMessage.SendTileSquare(Main.myPlayer, posX, posY, 2, 3, TileChangeType.None);

				//SpawnDustAtTile(i - offsetX, j - offsetY, DustID.PinkFairy);//debug
				return true;
			}
			return false;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			//these 2 values are for getting the right rotation point
			float Xoffset = Projectile.spriteDirection == 1 ? (tex.Width * 0.125f) : (tex.Width * 0.875f);
			Vector2 rotPoint = new Vector2(Xoffset, frameHeight * 0.5f);

			Main.spriteBatch.Draw(tex, 
					Projectile.position + 
					rotPoint + 
					new Vector2(Projectile.width / 2 - tex.Width / 2 - 0.6f,//weird specific offset to make it symetrical
					 -Owner.gfxOffY * 0.5f) - //this value is weird and sometimes breaks for no reason?
					Main.screenPosition, 
				frameBox, 
				lightColor, 
				Projectile.rotation, 
				rotPoint, 
				Projectile.scale, 
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 
				0f);

			return false;
		}
	}
}