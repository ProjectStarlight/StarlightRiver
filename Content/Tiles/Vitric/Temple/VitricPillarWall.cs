using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class VitricPillarWall : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWall";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 25, DustType<Dusts.Sand>(), SoundID.Tink, new Color(54, 48, 42));
		}
	}

	class VitricPillarWallItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallItem() : base("Vitric Forge Pillar", "Sturdy", "VitricPillarWall", ItemRarityID.White) { }
	}

	class VitricPillarWallShort : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallShort";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 11, DustType<Dusts.Sand>(), SoundID.Tink, new Color(54, 48, 42));
		}
	}

	class VitricPillarWallShortItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallShortItem() : base("Short Vitric Forge Pillar", "Sturdy", "VitricPillarWallShort", ItemRarityID.White) { }
	}

	class VitricPillarWallLava : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallLava";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 25, DustType<Dusts.Sand>(), SoundID.Tink, new Color(54, 48, 42));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			Vector2 pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition;
			float sin = 0.5f + (float)Math.Sin((Main.GameUpdateCount + i + j * 10) * 0.05f) * 0.25f;

			spriteBatch.Draw(tex, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White * sin);

			Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(0.5f, 0.25f, 0) * sin);
		}
	}

	class VitricPillarWallLavaItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallLavaItem() : base("Lava Vitric Forge Pillar", "Sturdy", "VitricPillarWallLava", ItemRarityID.White) { }
	}

	class VitricPillarWallCrystal : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallCrystal";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 4, 25, DustType<Dusts.Sand>(), SoundID.Tink, new Color(54, 48, 42));
		}
	}

	class VitricPillarWallCrystalItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallCrystalItem() : base("Crystal Vitric Forge Pillar", "Sturdy", "VitricPillarWallCrystal", ItemRarityID.White) { }
	}
}