using Mono.Cecil.Cil;
using StarlightRiver.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Starlight
{
	class OracleGlass : ModTile
	{
		public override string Texture => AssetDirectory.StarlightTile + "OracleGlass";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, int.MaxValue, DustID.Glass, SoundID.Shatter, new Color(30, 70, 222), ModContent.ItemType<OracleGlassItem>());
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;
			MinPick = 100;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Framing.SelfFrame8Way(i, j, Main.tile[i, j], resetFrame);

			Tile tile = Main.tile[i, j];
			tile.TileFrameX += (short)(i % 6 * 324);
			tile.TileFrameY += (short)(j % 6 * 90);

			return false;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.2f, 0.4f, 1f);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];

			var tex = Assets.Tiles.Starlight.OracleGlassGlow.Value;
			var frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
			Vector2 target = new Vector2(i, j) * 16 + Vector2.One * Main.offScreenRange - Main.screenPosition + Vector2.One * 8;
			var color = new Color(255, 255, 255, 0);

			float time = Main.GameUpdateCount * 0.01f + i * 0.2f;
			float time2 = Main.GameUpdateCount * 0.01f + j * 0.2f;
			float prog = 0.5f + (MathF.Sin(time) + MathF.Sin(time * 1.7f - 5) + MathF.Sin(time * 1.2f - 2)) * 0.2f;
			prog += (MathF.Sin(time2 * 0.8f) + MathF.Sin(time2 * 1.1f + 2) + MathF.Sin(time2 * 1.6f - 3)) * 0.2f;
			color *= prog;

			spriteBatch.Draw(tex, target, frame, color, 0, Vector2.One * 8, 1, 0, 0);
		}
	}

	class OracleGlassItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.StarlightTile + Name;

		public OracleGlassItem() : base("Oracle Glass", "", "OracleGlass", ItemRarityID.White) { }
	}
}