using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;

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
		public static HashSet<int> grayEmissionTypes = new();

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
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileMerge[Type][TileID.CrimsonGrass] = true;
			Main.tileMerge[TileID.CrimsonGrass][Type] = true;

			HitSound = Terraria.ID.SoundID.NPCHit1;
			DustType = Terraria.ID.DustID.Blood;

			MinPick = 101;

			RegisterItemDrop(ModContent.ItemType<GrayMatterItem>());

			AddMapEntry(new Color(167, 180, 191));
			grayEmissionTypes.Add(Type);
		}

		private void DrawTileMap(SpriteBatch spriteBatch)
		{
			if (!StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen))
				return;

			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			var pos = (Main.screenPosition / 16).ToPoint16();

			int width = Main.screenWidth / 16 + 1;
			int height = Main.screenHeight / 16 + 1;

			for (int x = pos.X; x < pos.X + width; x++)
			{
				for (int y = pos.Y; y < pos.Y + height; y++)
				{
					Point16 target = new Point16(x, y);
					Tile tile = Main.tile[target];

					if (grayEmissionTypes.Contains(tile.TileType))
					{
						Vector2 drawPos = target.ToVector2() * 16 + Vector2.One * 8 - Main.screenPosition;
						Color color = Color.White * 0.7f;
						color.A = 0;

						// Draw to map
						spriteBatch.Draw(glow, drawPos, null, color, 0, glow.Size() / 2f, 1.1f + 0.4f * MathF.Sin(Main.GameUpdateCount * 0.05f + (target.X ^ target.Y)), 0, 0);
					}
				}
			}
		}

		private void DrawRealVersion(SpriteBatch spriteBatch, int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);

			if (grayEmissionTypes.Contains(tile.TileType))
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.Tile[tile.TileType].Value;
				spriteBatch.Draw(tex, new Vector2(x, y) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White * 0.1f);
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			//(r, g, b) = (0.3f, 0.3f, 0.3f);
		}

		public override void FloorVisuals(Player player)
		{
			if (StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen))
				player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 180);
		}
	}
}