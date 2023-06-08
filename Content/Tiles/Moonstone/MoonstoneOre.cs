using System;
using Terraria.ID;
using StarlightRiver.Content.Abilities;
using static Terraria.ModLoader.ModContent;
using Terraria;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	public class MoonstoneOre : ModTile, IHintable
	{
		public override string Texture => AssetDirectory.MoonstoneTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSet(50, DustType<Dusts.Stone>(), SoundID.Tink, new Color(64, 71, 89), ItemType<Items.Moonstone.MoonstoneOreItem>(), true, true, "Moonstone Ore");
		}
#pragma warning disable IDE0047 // Remove unnecessary parentheses
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			//Utils.DrawBorderString(spriteBatch, temp.ToString(), (new Vector2(i + 12, j + 7 - (i % 4)) * 16) - Main.screenPosition, Color.White, 0.75f);
			for (int k = j - 1; k > j - 4; k--)//checks 3 blocks above
			{
				if (Main.tile[i, k].HasTile && WorldGen.SolidOrSlopedTile(i, k) && !TileID.Sets.NotReallySolid[Main.tile[i, k].TileType])
					return true;
			}

			Color overlayColor = new Color(0.12f, 0.135f, 0.23f, 0f) * (2 * (((float)Math.Sin(Main.GameUpdateCount * 0.02f) + 4) / 4));
			float heightScale = (float)Math.Sin(Main.GameUpdateCount * 0.025f) / 8 + 1;

			//unsure if these are needed
			float yOffsetLeft = 0;
			float yOffsetRight = 0;
			switch (Main.tile[i, j].Slope)
			{
				case SlopeType.SlopeDownLeft:// '\' slope
					yOffsetLeft = 1f;
					break;

				case SlopeType.SlopeDownRight:// '/' slope 
					yOffsetRight = 1f;
					break;
			}

			//each of these refer to the 1x3 area on each side of the tile
			bool AnyOnRight = false;//if there are any tiles in the 1x3 on the side, this is false if there is a tile above this area
			int RightHighest = 3;//-2 means no tile, can be any negative number below -1

			bool AnyOnLeft = false;
			int LeftHighest = 3;

			for (int k = j + 1; k > j - 2; k--)//scans 3 blocks vertically, starting 1 lower on each side
			{
				bool tileRight = Main.tile[i + 1, k].HasTile && WorldGen.SolidOrSlopedTile(i + 1, k) && Main.tile[i + 1, k].TileType == Type;
				AnyOnRight = AnyOnRight || tileRight;
				RightHighest = tileRight ? k - j : RightHighest;//sets this value to the highest tile coordinate

				bool tileLeft = Main.tile[i - 1, k].HasTile && WorldGen.SolidOrSlopedTile(i - 1, k) && Main.tile[i - 1, k].TileType == Type;
				AnyOnLeft = AnyOnLeft || tileLeft;
				LeftHighest = tileLeft ? k - j : LeftHighest;
			}

			//extra checks in case this is next to a wall
			if(Main.tile[i + 1, j - 2].HasTile && WorldGen.SolidOrSlopedTile(i + 1, j - 2))//if there is a tile on top of 3x3 area
			{
				RightHighest = 3;
				AnyOnRight = false;
			}

			if (Main.tile[i - 1, j - 2].HasTile && WorldGen.SolidOrSlopedTile(i - 1, j - 2))
			{
				LeftHighest = 3;
				AnyOnLeft = false;
			}

			Texture2D midTex;
			bool slopes = false;//if this uses a sloped version of the sprite

			if ((!AnyOnLeft || !AnyOnRight || RightHighest + LeftHighest <= 0) && RightHighest + LeftHighest > -2 && RightHighest + LeftHighest < 3 && RightHighest != LeftHighest)//if this should slope
			{
				slopes = true;
				if (RightHighest > LeftHighest)
				{
					midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSlopeRightHalf").Value;
				}
				else
				{
					midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowSlopeLeftHalf").Value;
				}
			}
			else
			{
				midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowMid").Value;
			}

			bool fillInBottomGap = Main.tile[i, j].IsHalfBlock || Main.tile[i, j].TopSlope;
			int stepUp = ((AnyOnRight || AnyOnLeft) && RightHighest + LeftHighest != -2) ? 1 : 5;
			var frame = new Rectangle(0, 0, 16, ((88 - stepUp * 8 + 8 * (j % stepUp)))  + (slopes ? 15 : 0) + (fillInBottomGap ? 5 : 0));//one pixel off because scaling issues
			spriteBatch.Draw(midTex, new Vector2(i + 12, j + 12.5f + (fillInBottomGap ? 0.625f/*one extra pixel than half a block*/ : 0)) * 16 - Main.screenPosition, frame, overlayColor, default, new Vector2(0, frame.Height), new Vector2(1, 2), default, default);

			//Utils.DrawBorderString(spriteBatch, RightHighest.ToString(), (new Vector2(i + 12, j + 7 - (i % 4)) * 16) - Main.screenPosition, Color.Red, 0.75f);
			//Utils.DrawBorderString(spriteBatch, LeftHighest.ToString(), (new Vector2(i + 12, j + 8 - (i % 4)) * 16) - Main.screenPosition, Color.Green, 0.75f);
			//Utils.DrawBorderString(spriteBatch, stepUp.ToString(), (new Vector2(i + 12, j + 9 - (i % 4)) * 16) - Main.screenPosition, Color.Blue, 0.75f);


			Texture2D glowLines = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowLines", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			int realX = i * 16;
			int realY = (int)((j + yOffsetLeft + yOffsetRight) * 16);
			int realWidth = glowLines.Width - 1; //1 pixel offset since the texture has a empty row of pixels on the side, this is also accounted for elsewhere below
			Color drawColor = overlayColor * 0.35f;

			float val = (Main.GameUpdateCount * 0.3333f + realY) % realWidth;
			int offset = (int)(val + realX % realWidth - realWidth);

			//spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, 16, glowLines.Height), new Rectangle(offset + 1, 0, 16, (int)(glowLines.Height * heightScale)), drawColor);

			if (offset < 0)
			{
				int rectWidth = Math.Min(-offset, 16);
				//spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, rectWidth, glowLines.Height), new Rectangle(offset + 1 + realWidth, 0, rectWidth, (int)(glowLines.Height * heightScale)), drawColor);
			}

			return true;
		}

#pragma warning restore IDE0047 // Remove unnecessary parentheses

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Vector2 pos = new Vector2(i, j) * 16;
			Lighting.AddLight(pos, new Vector3(0.1f, 0.32f, 0.5f) * 0.35f);

			if (Main.rand.NextBool(50))
			{
				if (!Main.tile[i, j - 1].HasTile)
				{
					Dust.NewDustPerfect(pos + new Vector2(Main.rand.NextFloat(0, 16), Main.rand.NextFloat(-32, -16)),
						ModContent.DustType<Content.Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.02f, 0.02f), -Main.rand.NextFloat(0.1f, 0.36f)), 0, new Color(0.2f, 0.2f, 0.25f, 0f), Main.rand.NextFloat(0.25f, 0.5f));
				}
			}
		}

		public override void FloorVisuals(Player player)
		{
			player.AddBuff(BuffType<Buffs.Overcharge>(), 120);
		}

		public string GetHint()
		{
			return "This ore is full of Starlight, but... wrong. Twisted.";
		}
	}
}