using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class ThickTree : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Large Tree");

			Main.tileAxe[Type] = true;
			AddMapEntry(new Color(169, 125, 93), name);

			ItemDrop = ItemID.Wood;
		}

		private float GetLeafSway(float offset, float magnitude, float speed)
		{
			return (float)Math.Sin(Main.GameUpdateCount * speed + offset) * magnitude;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (right && !up && down || !up && !down)
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(new Point(i, j));
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (right && !up && down)
			{
				Texture2D tex = ModContent.Request<Texture2D>(Texture + "Top").Value;
				Vector2 pos = (new Vector2(i + 1, j) + Helpers.Helper.TileAdj) * 16;

				Color color = Lighting.GetColor(i, j);

				spriteBatch.Draw(tex, pos - Main.screenPosition, null, color, GetLeafSway(3, 0.05f, 0.008f), new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);

				Texture2D tex2 = ModContent.Request<Texture2D>(AssetDirectory.ForestTile + "Godray").Value;
				var godrayColor = new Color();
				float godrayRot = 0;

				if (Main.dayTime)
				{
					godrayColor = new Color(255, 255, 200) * 0.5f;
					godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 54000f * 3.14f), 3);
					godrayRot = -0.5f * 1.57f + (float)Main.time / 54000f * 3.14f;
				}
				else
				{
					godrayColor = new Color(200, 210, 255) * 0.5f;
					godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 24000f * 3.14f), 3) * 0.25f;
					godrayRot = -0.5f * 1.57f + (float)Main.time / 24000f * 3.14f;
				}

				godrayColor.A = 0;

				pos += new Vector2(0, -100);

				int daySeed = i + (int)Main.GetMoonPhase();

				if (daySeed % 3 == 0)
					spriteBatch.Draw(tex2, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 0.85f, 0, 0);

				pos += new Vector2(-60, 80);

				if (daySeed % 5 == 0)
					spriteBatch.Draw(tex2, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 0.65f, 0, 0);

				pos += new Vector2(150, -60);

				if (daySeed % 7 == 0)
					spriteBatch.Draw(tex2, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 0.75f, 0, 0);

			}

			if (!up && !down)
			{
				Texture2D sideTex = Terraria.GameContent.TextureAssets.TreeTop[0].Value;
				Vector2 sidePos = (new Vector2(i + 1, j) + Helpers.Helper.TileAdj) * 16;

				if (left)
					spriteBatch.Draw(sideTex, sidePos + new Vector2(20, 0) - Main.screenPosition, null, Color.White, 0, Vector2.Zero, 1, 0, 0);

				if (right)
					spriteBatch.Draw(sideTex, sidePos + new Vector2(0, 20) - Main.screenPosition, null, Color.White, 0, Vector2.Zero, 1, 0, 0);
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (right && !up && down)
			{
				Texture2D tex = ModContent.Request<Texture2D>(Texture + "Top").Value;
				Vector2 pos = (new Vector2(i + 1, j) + Helpers.Helper.TileAdj) * 16;

				Color color = Lighting.GetColor(i, j);

				spriteBatch.Draw(tex, pos + new Vector2(50, 40) - Main.screenPosition, null, color.MultiplyRGB(Color.Gray), GetLeafSway(0, 0.05f, 0.01f), new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
				spriteBatch.Draw(tex, pos + new Vector2(-30, 80) - Main.screenPosition, null, color.MultiplyRGB(Color.DarkGray), GetLeafSway(2, 0.025f, 0.012f), new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
			}

			return true;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (Main.rand.NextBool(20) && right && !up && down)
			{
				if (Main.dayTime && Main.time > 10000 && Main.time < 44000)
				{
					float godrayRot = (float)Main.time / 54000f * 3.14f;
					Dust.NewDustPerfect(new Vector2(i, j) * 16 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100), ModContent.DustType<Dusts.GoldSlowFade>(), Vector2.UnitX.RotatedBy(godrayRot) * Main.rand.NextFloat(0.25f, 0.5f), 255, default, 0.75f);
				}
			}
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (left)
				WorldGen.KillTile(i - 1, j);
			if (right)
				WorldGen.KillTile(i + 1, j);
			if (up)
				WorldGen.KillTile(i, j - 1);
			if (down)
				WorldGen.KillTile(i, j - 1);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			short x = 0;
			short y = 0;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (up || down)
			{
				if (right)
					x = 0;

				if (left)
					x = 18;

				y = (short)(Main.rand.Next(3) * 18);

				if (Main.rand.NextBool(3))
					x += 18 * 2;
			}

			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = x;
			tile.TileFrameY = y;

			return false;
		}
	}

	class ThickTreeBase : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);

			this.QuickSetFurniture(4, 4, 0, SoundID.Dig, false, new Color(169, 125, 93));
		}
	}
}