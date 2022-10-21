using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	public class OvergrowthLivingWoodTile : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			HitSound = SoundID.Dig;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(125, 70, 40));

			ItemDrop = ModContent.ItemType<OvergrowthLivingWoodItem>();

			DustType = ModContent.DustType<OvergrowthLivingWoodDust>();

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Overgrown Living Wood");
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
		{
			frameXOffset = GetFrame(i, j) * 288; //width of texture divided by 2, the amount of "frames" for the tile
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			int frame = GetFrame(i, j);
			int xPosition = tile.TileFrameX + frame * 288;
			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
			Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			if (!tile.IsHalfBlock && tile.Slope == 0)
				spriteBatch.Draw(glow, drawOffset, new Rectangle(xPosition, tile.TileFrameY, 18, 18), Color.White * 0.5f, 0f, Vector2.Zero, 1f, 0f, 0f);
			else if (tile.IsHalfBlock)
				spriteBatch.Draw(glow, drawOffset + new Vector2(0f, 8f), new Rectangle(xPosition, tile.TileFrameY, 18, 8), Color.White * 0.5f, 0f, Vector2.Zero, 1f, 0f, 0f);
		}

		private int GetFrame(int i, int j)
		{
			int frame = 0;
			int xPosition = i % 4;
			int yPosition = j % 4;
			switch (xPosition)
			{
				case 0:
					frame = yPosition switch
					{
						0 => 0,
						1 => 1,
						2 => 0,
						3 => 0,
						_ => 0,
					};
					break;
				case 1:
					frame = yPosition switch
					{
						0 => 0,
						1 => 1,
						2 => 1,
						3 => 0,
						_ => 0,
					};
					break;
				case 2:
					frame = yPosition switch
					{
						0 => 1,
						1 => 0,
						2 => 0,
						3 => 1,
						_ => 0,
					};
					break;
				case 3:
					frame = yPosition switch
					{
						0 => 1,
						1 => 0,
						2 => 0,
						3 => 1,
						_ => 0,
					};
					break;
			}

			return frame;
		}
	}

	public class OvergrowthLivingWoodItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;
		public OvergrowthLivingWoodItem() : base("Overgrown Living Wood", "[PH] make this a wand and stuffs", "OvergrowthLivingWoodTile") { }
	}

	class OvergrowthLivingWoodDust : ModDust
	{
		public override string Texture => AssetDirectory.OvergrowTile + Name;

		public override void SetStaticDefaults()
		{
			UpdateType = DustID.t_LivingWood;
		}
	}
}