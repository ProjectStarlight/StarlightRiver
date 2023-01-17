using System;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Trophies
{
	class AuroracleTrophy : ModTile
	{
		public override string Texture => AssetDirectory.TrophyTile + "TrophyGeneric";

		public override void SetStaticDefaults()
		{
			this.QuickSetPainting(3, 3, 7, new Color(120, 85, 60), "Trophy");
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX == 2 * 18 && tile.TileFrameY == 2 * 18)
			{
				Texture2D headBlob = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOver").Value;
				Texture2D headBlobGlow = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOverGlow").Value;
				Texture2D headBlobSpecular = Request<Texture2D>(AssetDirectory.SquidBoss + "BodyOverSpecular").Value;

				Texture2D tex2 = Request<Texture2D>(AssetDirectory.TrophyTile + Name + "Glow2").Value;

				Vector2 pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Vector2.One * 8 - Main.screenPosition;

				float sin = 1 + (float)Math.Sin(Main.GameUpdateCount * 0.05f);
				float cos = 1 + (float)Math.Cos(Main.GameUpdateCount * 0.05f);
				float scale = 0.8f;

				Color color = new Color(0.5f + cos * 0.25f, 0.8f, 0.5f + sin * 0.25f).MultiplyRGB(Lighting.GetColor(i - 1, j - 1));

				var frame = new Rectangle(0, 22, 64, 38);

				spriteBatch.Draw(headBlob, pos, frame, color * 0.8f, 0, frame.Size() / 2, scale, 0, 0);

				color.A = 0;
				spriteBatch.Draw(headBlobGlow, pos, frame, color * 0.6f, 0, frame.Size() / 2, scale, 0, 0);
				spriteBatch.Draw(headBlobSpecular, pos, frame, Lighting.GetColor(i - 1, j - 1) * 1.5f, 0, frame.Size() / 2, scale, 0, 0);

				spriteBatch.Draw(tex2, pos, null, color * 0.2f, 0, tex2.Size() / 2, scale * 0.26f, 0, 0);
				spriteBatch.Draw(tex2, pos, null, color * 0.075f, 0, tex2.Size() / 2, scale * 0.32f, 0, 0);
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<AuroracleTrophyItem>());
		}
	}

	class AuroracleTrophyItem : QuickTileItem
	{
		public AuroracleTrophyItem() : base("Auroracle Trophy", "", "AuroracleTrophy", 0, AssetDirectory.TrophyTile + "TrophyGenericItem", true, 0) { }

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
		}
	}
}
