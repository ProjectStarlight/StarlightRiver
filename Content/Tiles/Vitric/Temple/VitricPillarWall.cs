using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class VitricPillarWall : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWall";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 0, DustType<Dusts.Sand>(), SoundID.Dig, new Color(54, 48, 42), ItemType<VitricTempleWallItem>());
			Main.tileSolid[Type] = false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricPillarWall").Value;
			Vector2 pos = (new Vector2(i + 0.5f, j + 1) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition;
			spriteBatch.Draw(tex, pos, null, Lighting.GetColor(new Point(i, j)), 0, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);

			return false;
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
			QuickBlock.QuickSet(this, 0, DustType<Dusts.Sand>(), SoundID.Dig, new Color(54, 48, 42), ItemType<VitricTempleWallItem>());
			Main.tileSolid[Type] = false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricPillarWallShort").Value;
			Vector2 pos = (new Vector2(i + 0.5f, j + 1) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition;
			spriteBatch.Draw(tex, pos, null, Lighting.GetColor(new Point(i, j)), 0, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);

			return false;
		}
	}

	class VitricPillarWallShortItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricPillarWallItem";

		public VitricPillarWallShortItem() : base("Short Vitric Forge Pillar", "Sturdy", "VitricPillarWallShort", ItemRarityID.White) { }
	}
}
