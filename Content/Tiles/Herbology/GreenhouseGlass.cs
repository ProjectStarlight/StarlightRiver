using StarlightRiver.Content.Items.Moonstone;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Herbology
{
	public class GreenhouseGlass : ModTile
	{
		public override string Texture => AssetDirectory.HerbologyTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSet(0, 13, SoundID.Shatter, new Color(156, 172, 177), ModContent.ItemType<GreenhouseGlassItem>(), false, false, "Greenhouse Glass");
			Main.tileBlockLight[Type] = false;
			//Main.tileLighted[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
		}

		//public override bool RightClick(int i, int j)
		//{
		//	Main.NewText("Manual Update");
		//	ModContent.GetModTile(Main.tile[i, j].TileType)?.RandomUpdate(i, j);
		//	return base.RightClick(i, j);
		//}

		public override void RandomUpdate(int i, int j)
		{
			const int SkyRangeUp = 10;
			for (int k = 0; k < SkyRangeUp; k++)//k = max range up, this checks the area above it
			{
				if (Main.tile[i, j - 1 - k].HasTile && Main.tileBlockLight[Main.tile[i, j - 1 - k].TileType])
				{
					break;//breaks if a light blocking block is found
				}
				else if (k == 9)//starts downward scan on last block checked
				{
					const int PlantRangeDown = 10;
					for (int m = 0; m < PlantRangeDown; m++)//k = max range down, if the area above it clear this looks for the first plant below it
						if (Main.tile[i, j + 1 + m].HasTile && Main.tileSolid[Main.tile[i, j + 1 + m].TileType] && !Main.tileSolidTop[Main.tile[i, j + 1 + m].TileType])
							break;//breaks if Solid is true, Active is true, and solidTop is false
						else if (
							Main.tile[i, j + 1 + m].HasTile &&
							Main.tileFrameImportant[Main.tile[i, j + 1 + m].TileType] &&
							!Main.tileSolid[Main.tile[i, j + 1 + m].TileType])//chooses if frameimportant, non-solid, and active
						{
							ModTile modtile = ModContent.GetModTile(Main.tile[i, j + 1 + m].TileType);
							if (modtile is not null)
							{
								//if (Main.rand.NextBool(2))//50% chance to effect modded plant
								//{
									ModContent.GetModTile(Main.tile[i, j + 1 + m].TileType)?.RandomUpdate(i, j + 1 + m);
									NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
								//}
							}
							else//vanilla grow check has seperate changes
								GrowVanillaPlant(i, j + 1 + m);
							break;
						}
				}
			}
		}

		public void GrowVanillaPlant(int i, int j)
		{
			int type = Main.tile[i, j].TileType;
			const int baseChance = 10;//chance when plant conditions are not met
			//tehse chances are not really based on much 
			switch (type)
			{
				//vanilla dye plants have seperate tiles for each stage, but share the same tile for each plant type
				case 82://first stage
					{
						Main.tile[i, j].TileType = 83;
						NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
					}
					break;
				case 83://last stage before bloom
					{
						int tileFrameX = Main.tile[i, j].TileFrameX;
						switch (tileFrameX)
						{
							case 0://daybloom
								if (Main.IsItDay())
								{
									if (Main.rand.NextBool(3))
										Main.tile[i, j].TileType = 84;
								}
								else if (Main.rand.NextBool(baseChance))
									Main.tile[i, j].TileType = 84;
								break;

							case 18://moonglow
								if (!Main.IsItDay())
								{
									if (Main.rand.NextBool(3))
										Main.tile[i, j].TileType = 84;
								}
								else if (Main.rand.NextBool(baseChance))
									Main.tile[i, j].TileType = 84;
								break;

							case 36://blinkroot
								if (Main.rand.NextBool(6))
									Main.tile[i, j].TileType = 84;
								break;

							case 54://deathweed
								if (Main.bloodMoon || Main.GetMoonPhase() == Terraria.Enums.MoonPhase.Full)
								{
									if (Main.rand.NextBool(2))
										Main.tile[i, j].TileType = 84;
								}
								else if (Main.rand.NextBool(baseChance))
									Main.tile[i, j].TileType = 84;
								break;


							case 72://waterleaf
								if (Main.raining || Main.tile[i, j].LiquidType == LiquidID.Water && Main.tile[i, j].LiquidAmount > 0)
								{
									if (Main.rand.NextBool(3))
										Main.tile[i, j].TileType = 84;
								}
								else if (Main.rand.NextBool(baseChance))
									Main.tile[i, j].TileType = 84;
								break;

							case 90://fireblossom
								if (Main.tile[i, j].LiquidType == LiquidID.Lava && Main.tile[i, j].LiquidAmount > 0)//vanilla does not use the lava check anymore
								{
									if (Main.rand.NextBool(2))
										Main.tile[i, j].TileType = 84;
								}
								else  if (!Main.raining && Main.IsItDay())//vanilla uses sunset instead of daytime
								{
									if (Main.rand.NextBool(4))
										Main.tile[i, j].TileType = 84;
								}
								else if (Main.rand.NextBool(baseChance))
									Main.tile[i, j].TileType = 84;
								break;

							case 108://shiverthorn
								if (Main.rand.NextBool(8))//wiki just says 'after enough time has passed', so chance here is lower than blinkroot which says 'at random'
									Main.tile[i, j].TileType = 84;
								break;

							default:
								Main.tile[i, j].TileType = 84;
								break;
						}
						NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
					}
					break;
				case TileID.Bamboo: //assumes top block of bamboo /broken
					{
						if (Main.rand.NextBool(3))
						{
							if(!Main.tile[i, j - 1].HasTile)
							{
								//WorldGen.PlaceTile(i, j - 1, TileID.Bamboo, true, true);//does not work for some reason
								Main.tile[i, j - 1].Get<TileWallWireStateData>().HasTile = true;
								Main.tile[i, j - 1].Get<TileTypeData>().Type = TileID.Bamboo;
								WorldGen.SquareTileFrame(i, j - 1, true);
								NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
							}
						}
					}
					break;
				case TileID.Pumpkins: //does not assume top block
					{
						if (Main.rand.NextBool(3))
						{
							int sheetFrameX = Main.tile[i, j].TileFrameX / 18;
							int offsetX = sheetFrameX % 2;

							if (sheetFrameX < 8) 
							{
								int sheetFrameY = Main.tile[i, j].TileFrameY / 18;
								int offsetY = sheetFrameY % 2;

								for (int x = 0; x < 2; x++)
								{
									for (int y = 0; y < 2; y++)
									{
										Main.tile[i + x - offsetX, j + y - offsetY].TileFrameX += 36;
									}
								}
								NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
							}
						}
					}
					break;
			}
		}
	}

	public class GreenhouseGlassItem : QuickTileItem
	{
		public GreenhouseGlassItem() : base("Greenhouse Glass", "Speeds up the growth of any plant below it\nNeeds a 10 blocks of clear area or transparent blocks above it", "GreenhouseGlass", 1, AssetDirectory.HerbologyTile) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(4);
			recipe.AddIngredient(ItemID.Glass, 4);
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<GreenhouseWallItem>(), 4);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}

	public class GreenhouseWall : ModWall
	{
		public override string Texture => AssetDirectory.HerbologyTile + Name;

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			RegisterItemDrop(ModContent.ItemType<GreenhouseWallItem>());
		}
	}

	public class GreenhouseWallItem : QuickWallItem
	{
		public GreenhouseWallItem() : base("Greenhouse Glass Wall", "Fancy!", ModContent.WallType<GreenhouseWall>(), 0, AssetDirectory.HerbologyTile) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(4);//1 tile to 4 wall
			recipe.AddIngredient(ModContent.ItemType<GreenhouseGlassItem>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}