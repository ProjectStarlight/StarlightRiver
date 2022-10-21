using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	public class MoonstoneOre : ModTile
	{
		public override string Texture => AssetDirectory.MoonstoneTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSet(50, DustType<Dusts.Electric>(), SoundID.Tink, new Color(64, 71, 89), ItemType<Items.Moonstone.MoonstoneOreItem>(), true, true, "Moonstone Ore");
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			//Utils.DrawBorderString(spriteBatch, temp.ToString(), (new Vector2(i + 12, j + 7 - (i % 4)) * 16) - Main.screenPosition, Color.White, 0.75f);

			if (!Main.tile[i, j - 1].HasTile)
			{
				Color overlayColor = new Color(0.12f, 0.135f, 0.23f, 0f) * (((float)Math.Sin(Main.GameUpdateCount * 0.02f) + 4) / 4);
				float heightScale = (float)Math.Sin(Main.GameUpdateCount * 0.025f) / 8 + 1;

				bool emptyLeft;
				bool emptyRight;
				Texture2D midTex;
				float yOffsetLeft = 0;
				float yOffsetRight = 0;

				switch (Main.tile[i, j].Slope)
				{
					case SlopeType.SlopeDownLeft:// '\' slope
						Tile tileLeft0 = Main.tile[i - 1, j];
						Tile tileRight0 = Main.tile[i + 1, j + 1];

						emptyLeft = !(tileLeft0.HasTile && tileLeft0.TileType == Type && !Main.tile[i - 1, j - 1].HasTile ||
							Main.tile[i - 1, j - 1].Slope == SlopeType.SlopeDownLeft && Main.tile[i - 1, j - 1].TileType == Type && !Main.tile[i - 1, j - 2].HasTile);

						emptyRight = !tileRight0.HasTile || tileRight0.TileType != Type || tileRight0.Slope == SlopeType.SlopeDownRight || Main.tile[i + 1, j].HasTile;

						midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSlopeRight").Value;
						yOffsetLeft = 1f;
						break;

					case SlopeType.SlopeDownRight:// '/' slope
						Tile tileLeft1 = Main.tile[i - 1, j + 1];
						Tile tileRight1 = Main.tile[i + 1, j];

						emptyLeft = !tileLeft1.HasTile || tileLeft1.TileType != Type || tileLeft1.Slope == SlopeType.SlopeDownLeft || Main.tile[i - 1, j].HasTile;

						emptyRight = !(tileRight1.HasTile && tileRight1.TileType == Type && !Main.tile[i + 1, j - 1].HasTile ||
							Main.tile[i + 1, j - 1].Slope == SlopeType.SlopeDownRight && Main.tile[i + 1, j - 1].TileType == Type && !Main.tile[i + 1, j - 2].HasTile);

						midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSlopeLeft").Value;
						yOffsetRight = 1f;
						break;

					default:
						Tile tileLeft2 = Main.tile[i - 1, j];
						Tile tileRight2 = Main.tile[i + 1, j];

						emptyLeft = !(tileLeft2.HasTile && tileLeft2.TileType == Type && tileLeft2.Slope != SlopeType.SlopeDownLeft && !Main.tile[i - 1, j - 1].HasTile ||
							Main.tile[i - 1, j - 1].Slope == SlopeType.SlopeDownLeft && Main.tile[i - 1, j - 1].TileType == Type && !Main.tile[i - 1, j - 2].HasTile);

						emptyRight = !(tileRight2.HasTile && tileRight2.TileType == Type && tileRight2.Slope != SlopeType.SlopeDownRight && !Main.tile[i + 1, j - 1].HasTile ||
							Main.tile[i + 1, j - 1].Slope == SlopeType.SlopeDownRight && Main.tile[i + 1, j - 1].TileType == Type && !Main.tile[i + 1, j - 2].HasTile);

						midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowMid").Value;
						break;
				}

				if (emptyLeft)
				{
					if (emptyRight) //solo
						spriteBatch.Draw(Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSolo").Value, new Vector2(i + 12, j + 7.5f + yOffsetLeft + yOffsetRight) * 16 - Main.screenPosition, overlayColor);
					else            //left
						spriteBatch.Draw(Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowLeft").Value, new Vector2(i + 12, j + 7.5f + yOffsetLeft) * 16 - Main.screenPosition, overlayColor);
				}
				else if (emptyRight)//right
				{
					spriteBatch.Draw(Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowRight").Value, new Vector2(i + 12, j + 7.5f + yOffsetRight) * 16 - Main.screenPosition, overlayColor);
				}
				else                //both
				{
					spriteBatch.Draw(midTex, new Vector2(i + 12, j + 7.5f) * 16 - Main.screenPosition, overlayColor);
				}

				Texture2D glowLines = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowLines").Value;
				int realX = i * 16;
				int realY = (int)((j + yOffsetLeft + yOffsetRight) * 16);
				int realWidth = glowLines.Width - 1;//1 pixel offset since the texture has a empty row of pixels on the side, this is also accounted for elsewhere below
				Color drawColor = overlayColor * 0.35f;

				float val = (Main.GameUpdateCount * 0.3333f + realY) % realWidth;
				int offset = (int)(val + realX % realWidth - realWidth);

				spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, 16, glowLines.Height), new Rectangle(offset + 1, 0, 16, (int)(glowLines.Height * heightScale)), drawColor);

				if (offset < 0)
				{
					int rectWidth = Math.Min(-offset, 16);
					spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, rectWidth, glowLines.Height), new Rectangle(offset + 1 + realWidth, 0, rectWidth, (int)(glowLines.Height * heightScale)), drawColor);
				}
			}

			return true;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Vector2 pos = new Vector2(i, j) * 16;
			Lighting.AddLight(pos, new Vector3(0.1f, 0.32f, 0.5f) * 0.35f);
			//Dust.NewDustDirect(pos, 16, 16, ModContent.DustType<Content.Dusts.MoonstoneShimmer>(), 0, 0, 0, Color.White, 0.05f);
			if (Main.rand.Next(50) == 0)
			{
				if (!Main.tile[i, j - 1].HasTile)
				{
					Dust.NewDustPerfect(pos + new Vector2(Main.rand.NextFloat(0, 16), Main.rand.NextFloat(-16, -8)),
						ModContent.DustType<Content.Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.02f, 0.02f), -Main.rand.NextFloat(0.05f, 0.18f)), 0, new Color(0.2f, 0.2f, 0.25f, 0f), Main.rand.NextFloat(0.25f, 0.5f));
				}
			}
		}

		public override void FloorVisuals(Player player)
		{
			player.AddBuff(BuffType<Buffs.Overcharge>(), 120);
		}
	}
}