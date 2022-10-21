using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class SetpieceAltar : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + "SetpieceAltar";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 10, 7, DustID.Stone, SoundID.Tink, true, new Color(100, 100, 80));
		}
	}
}