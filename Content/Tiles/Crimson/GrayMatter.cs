using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class GrayMatterItem : QuickTileItem
	{
		public GrayMatterItem() : base("Gray Matter", "You can see it thinking", "GrayMatter", 0, "StarlightRiver/Assets/Tiles/Crimson/") { }
	}

	internal class GrayMatter : ModTile
	{
		/// <summary>
		/// List of tiles with graymatter emissions. This isnt the most elegant solution and should probably go elsewhere? TODO: Migrate this somewhere
		/// </summary>
		public static List<int> grayEmissionTypes = new();

		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/" + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawTileMap;
			GraymatterBiome.onDrawOverPerTile += DrawRealVersion;		
		}

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;

			HitSound = Terraria.ID.SoundID.NPCHit1;
			DustType = Terraria.ID.DustID.Blood;

			MinPick = 101;

			RegisterItemDrop(ModContent.ItemType<GrayMatterItem>());

			AddMapEntry(new Color(167, 180, 191));
			grayEmissionTypes.Add(Type);
		}

		private void DrawTileMap(SpriteBatch spriteBatch)
		{
			Texture2D glow = Assets.Keys.GlowAlpha.Value;
			var pos = (Main.screenPosition / 16).ToPoint16();

			int width = Main.screenWidth / 16 + 1;
			int height = Main.screenHeight / 16 + 1;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Point16 target = pos + new Point16(x, y);

					var tileType = Framing.GetTileSafely(target).TileType;
					if (grayEmissionTypes.Contains(tileType))
					{
						Vector2 drawPos = target.ToVector2() * 16 + Vector2.One * 8 - Main.screenPosition;
						Color color = Color.White;
						color.A = 0;

						// Decrease opacity as more tiles are nearby to normalize gradient intensity for more consistent
						// effects between differnet sized chunks
						for (int x2 = -1; x2 <= 1; x2++)
						{
							for (int y2 = -1; y2 <= 1; y2++)
							{
								var adjTileType = Framing.GetTileSafely(target + new Point16(x2, y2)).TileType;
								if (grayEmissionTypes.Contains(adjTileType))
									color *= 0.82f;
							}
						}

						// Draw to map
						spriteBatch.Draw(glow, drawPos, null, color, 0, glow.Size() / 2f, 1.7f, 0, 0);
					}
				}
			}
		}

		private void DrawRealVersion(SpriteBatch spriteBatch, int x, int y)
		{
			var target = new Point16(x, y);
			Tile tile = Framing.GetTileSafely(target);

			if (grayEmissionTypes.Contains(tile.TileType))
			{
				if (tile.TileType == ModContent.TileType<GrayMatter>())
				{
					Texture2D tex = Assets.Tiles.Crimson.GrayMatterOver.Value;
					spriteBatch.Draw(tex, target.ToVector2() * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White);
				}
				else
				{
					Texture2D tex = Terraria.GameContent.TextureAssets.Tile[tile.TileType].Value;
					spriteBatch.Draw(tex, target.ToVector2() * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White);
				}
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.7f, 0.7f, 0.7f);
		}

		public override void FloorVisuals(Player player)
		{
			player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
		}
	}
}
