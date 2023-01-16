using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	public class MoonstoneOre : ModTile
	{
		public override string Texture => AssetDirectory.MoonstoneTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSet(50, DustType<Dusts.Stone>(), SoundID.Tink, new Color(64, 71, 89), ItemType<Items.Moonstone.MoonstoneOreItem>(), true, true, "Moonstone Ore");
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			//Utils.DrawBorderString(spriteBatch, temp.ToString(), (new Vector2(i + 12, j + 7 - (i % 4)) * 16) - Main.screenPosition, Color.White, 0.75f);

			for (int k = j - 1; k > j - 6; k--)
			{
				if (Main.tile[i, k].HasTile)
					return true;
			}

			Color overlayColor = new Color(0.12f, 0.135f, 0.23f, 0f) * (2 * (((float)Math.Sin(Main.GameUpdateCount * 0.02f) + 4) / 4));
			float heightScale = (float)Math.Sin(Main.GameUpdateCount * 0.025f) / 8 + 1;

			Texture2D midTex = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowMid").Value;
			float yOffsetLeft = 0;
			float yOffsetRight = 0;

			switch (Main.tile[i, j].Slope)
			{
				case SlopeType.SlopeDownLeft: // '\' slope
					yOffsetLeft = 1f;
					break;

				case SlopeType.SlopeDownRight: // '/' slope 
					yOffsetRight = 1f;
					break;
			}

			const int stepUp = 5;
			var frame = new Rectangle(0, 0, 16, 88 - stepUp * 8 + 8 * (j % stepUp));
			spriteBatch.Draw(midTex, new Vector2(i + 12, j + 12.5f) * 16 - Main.screenPosition, frame, overlayColor, default, new Vector2(0, frame.Height), new Vector2(1, 2), default, default);

			Texture2D glowLines = Request<Texture2D>(AssetDirectory.MoonstoneTile + "GlowLines", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			int realX = i * 16;
			int realY = (int)((j + yOffsetLeft + yOffsetRight) * 16);
			int realWidth = glowLines.Width - 1; //1 pixel offset since the texture has a empty row of pixels on the side, this is also accounted for elsewhere below
			Color drawColor = overlayColor * 0.35f;

			float val = (Main.GameUpdateCount * 0.3333f + realY) % realWidth;
			int offset = (int)(val + realX % realWidth - realWidth);

			spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, 16, glowLines.Height), new Rectangle(offset + 1, 0, 16, (int)(glowLines.Height * heightScale)), drawColor);

			if (offset < 0)
			{
				int rectWidth = Math.Min(-offset, 16);
				spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, rectWidth, glowLines.Height), new Rectangle(offset + 1 + realWidth, 0, rectWidth, (int)(glowLines.Height * heightScale)), drawColor);
			}

			return true;
		}

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
	}
}