using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Tiles.Overgrow
{
    internal class SetpieceAltar : ModTile { public override void SetDefaults() { QuickBlock.QuickSetFurniture(this, 10, 7, DustID.Stone, SoundID.Tink, true, new Color(100, 100, 80)); } }
}