using StarlightRiver.Content.Abilities;
using StarlightRiver.Helpers;
using System;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	class AuroraIce : ModTile, IHintable
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIce";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 0, -1, SoundID.Shatter, new Color(100, 255, 255), ItemType<AuroraIceItem>());
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;

			Main.tileMerge[TileID.IceBlock][Type] = true;
			Main.tileMerge[Type][TileID.IceBlock] = true;

			Main.tileMerge[TileID.SnowBlock][Type] = true;
			Main.tileMerge[Type][TileID.SnowBlock] = true;

			TileID.Sets.DrawsWalls[Type] = true;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (!fail && tile.TileFrameY < 4 * 18)
			{
				for (int k = 0; k < 3; k++)
				{
					float off = i + j;
					float time = Main.GameUpdateCount / 600f * 6.28f;

					float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
					float cos = (float)Math.Cos(time + off * 0.2f);
					var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
					if (color.R < 80)
						color.R = 80;
					if (color.G < 80)
						color.G = 80;

					var d = Dust.NewDustPerfect(new Vector2(i, j) * 16, DustType<Dusts.Crystal>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color * Main.rand.NextFloat(1.2f, 1.5f), Main.rand.NextFloat(0.4f, 0.6f));
					d.fadeIn = Main.rand.NextFloat(-0.1f, 0.1f);

					var d2 = Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(Main.rand.Next(16), Main.rand.Next(16)), DustType<Dusts.Aurora>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 100, color, 0);
					d2.customData = Main.rand.NextFloat(0.25f, 0.5f);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.2f, Pitch = -0.8f }, new Vector2(i, j) * 16f);

				if (CheckIce(i - 1, j) || CheckIce(i, j - 1) || CheckIce(i + 1, j) || CheckIce(i, j + 1))
				{
					tile.TileFrameY += 4 * 18;
					Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<AuroraIceItem>());
					fail = true;
				}
				else
				{
					tile.HasTile = false;
				}

				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						Tile tile2 = Framing.GetTileSafely(i + x, j + y);

						if (tile2.HasTile && tile2.TileType == Type && tile2.TileFrameY < 4 * 18)
							WorldGen.KillTile(i + x, j + y);
					}
				}
			}
			else
			{
				noItem = true;
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			//TODO: this is gross, change it later?
			if (CheckIce(i - 1, j) || CheckIce(i, j - 1) || CheckIce(i + 1, j) || CheckIce(i, j + 1))
				Framing.GetTileSafely(i, j).Slope = SlopeType.Solid;

			return false;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameY >= 4 * 18 || CheckIce(i - 1, j) || CheckIce(i, j - 1) || CheckIce(i + 1, j) || CheckIce(i, j + 1))
			{
				Color light = Lighting.GetColor(i, j);
				spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Permafrost/AuroraIceUnder").Value, (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY % (4 * 18), 16, 16), light);
			}

			if (tile.TileFrameY >= 4 * 18)
				return;

			int off = i + j;
			float time = Main.GameUpdateCount / 600f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			if (color.R < 80)
				color.R = 80;

			if (color.G < 80)
				color.G = 80;

			Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;
			var frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

			spriteBatch.Draw(TextureAssets.Tile[tile.TileType].Value, pos, frame, color * 0.3f);

			spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Permafrost/AuroraIce").Value, pos, frame, Color.Lerp(color, Color.White, 0.2f) * 0.1f);
			spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Permafrost/AuroraIceGlow2").Value, pos, frame, Color.Lerp(color, Color.White, 0.4f) * 0.4f);
			spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Permafrost/AuroraIceGlow").Value, pos, frame, Color.Lerp(color, Color.White, 0.7f) * 0.8f);

			if (Main.rand.NextBool(24))
			{
				var d = Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(Main.rand.Next(16), Main.rand.Next(16)), DustType<Dusts.Aurora>(), Vector2.Zero, 100, color, 0);
				d.customData = Main.rand.NextFloat(0.25f, 0.5f);
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameY >= 4 * 18)
				return;

			int off = i + j;
			float time = Main.GameUpdateCount / 600f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			if (color.R < 80)
				color.R = 80;

			if (color.G < 80)
				color.G = 80;

			(r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
		}

		bool CheckIce(int x, int y)
		{
			return Framing.GetTileSafely(x, y).TileType == TileID.IceBlock;
		}
		public string GetHint()
		{
			return "It fades away when you look at it...";
		}
	}

	//TODO: Move all this to a more sane place, im really tired tonight and cant be assed to put braincells into organizing this. Thanks in advance future me.
	class AuroraIceItem : QuickMaterial
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIceItem";

		public AuroraIceItem() : base("Frozen Aurora Chunk", "A preserved piece of the night sky", 999, Item.sellPrice(0, 0, 5, 0), ItemRarityID.White) { }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, position, frame, itemColor, 0, origin, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, position, frame, overColor * 0.75f, 0, origin, scale, 0, 0);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, Item.color * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, overColor * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			return false;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}

		public override void UpdateInventory(Player player)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}
	}

	class AuroraIceBar : QuickMaterial
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIceBar";

		public AuroraIceBar() : base("Frozen Aurora Bar", "A preserved selection of the night sky", 99, Item.sellPrice(0, 0, 25, 0), ItemRarityID.Blue) { }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, position, frame, itemColor, 0, origin, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, position, frame, overColor * 0.75f, 0, origin, scale, 0, 0);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, Item.color * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, overColor * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			return false;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}

		public override void UpdateInventory(Player player)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<AuroraIceItem>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();
		}
	}
}