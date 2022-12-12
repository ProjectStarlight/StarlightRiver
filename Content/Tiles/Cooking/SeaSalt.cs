using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Cooking
{
	class SeaSalt : ModTile
	{
		public override string Texture => AssetDirectory.CookingTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileMerge[Type][TileID.Sand] = true;
			Main.tileMerge[TileID.Sand][Type] = true;
			this.QuickSet(0, 2, SoundID.Dig, Color.White, ItemType<Items.Food.SeaSalt>());
		}
	}
}
