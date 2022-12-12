using StarlightRiver.Helpers;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Forest
{
	public class LeafWall : ModWall
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			WallID.Sets.Conversion.Grass[Type] = true;
			DustType = DustID.Grass;
			HitSound = SoundID.Grass;
			AddMapEntry(new Color(50, 140, 90));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (i > Main.screenPosition.X / 16 && i < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j > Main.screenPosition.Y / 16 && j < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
			{
				Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Forest/LeafWallFlow").Value;
				var rand = new Random(i * j % 192372);

				float offset = i * j % 6.28f + (float)rand.NextDouble() / 8f;
				float sin = (float)Math.Sin(Main.GameUpdateCount / 45f + offset);

				spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 2.2f - Main.screenPosition,
				new Rectangle(rand.Next(4) * 26, 0, 24, 24), Lighting.GetColor(i, j), offset + sin * 0.09f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);

				if (rand.Next(7) == 0)
				{
					Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Forest/LeafWallFlower").Value;
					spriteBatch.Draw(tex2, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 1.8f - Main.screenPosition,
						new Rectangle(i * j % 4 * 10, 0, 8, 8), Lighting.GetColor(i, j), offset + sin * 0.07f, new Vector2(4, 4), 1, 0, 0);
				}
			}
		}
	}
}