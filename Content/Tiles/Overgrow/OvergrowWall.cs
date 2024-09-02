﻿using StarlightRiver.Helpers;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	public class WallOvergrowGrass : ModWall
	{
		public override string Texture => AssetDirectory.OvergrowTile + "WallOvergrowGrass";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustType<Dusts.Leaf>(), SoundID.Grass, 0, false, new Color(114, 65, 37));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (i + 1 > Main.screenPosition.X / 16 && i - 1 < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j + 1 > Main.screenPosition.Y / 16 && j - 1 < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
			{
				Texture2D tex = Assets.Tiles.Overgrow.WallOvergrowGrassFlow.Value;
				float offset = i * j % 6.28f;
				float sin = (float)Math.Sin(StarlightWorld.visualTimer + offset);
				int variant = i * i * j % 4;

				spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 1.2f - Main.screenPosition,
					new Rectangle(variant * 26, 0, 24, 24), Lighting.GetColor(i, j) * (0.65f + sin * 0.05f), offset + sin * 0.06f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);
			}
		}
	}

	public class WallOvergrowBrick : ModWall
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustType<Dusts.Stone>(), SoundID.Tink, 0, false, new Color(62, 68, 55));
		}
	}

	public class WallOvergrowInvisible : ModWall
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetWall(this, DustType<Dusts.Stone>(), SoundID.Tink, 0, false, new Color(255, 235, 50));
			WallID.Sets.Transparent[Type] = true;
		}
	}

	public class WallOvergrowBrickItem : QuickWallItem
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;
		public WallOvergrowBrickItem() : base("Runic Brick Wall", "", WallType<WallOvergrowBrick>(), 0) { }
	}

	public class WallOvergrowGrassItem : QuickWallItem
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;
		public WallOvergrowGrassItem() : base("Overgrowth Grass Wall", "", WallType<WallOvergrowGrass>(), 0) { }
	}
}