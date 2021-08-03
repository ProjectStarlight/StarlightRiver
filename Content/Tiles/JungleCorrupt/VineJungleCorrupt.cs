using Microsoft.Xna.Framework;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleCorrupt
{
	public class VineJungleCorrupt : ModVine
    {
        public VineJungleCorrupt() : base(new string[] { "GrassJungleCorrupt" }, 14, new Color(64, 57, 94), 2, path: AssetDirectory.JungleCorruptTile) { }

        public override void NearbyEffects(int i, int j, bool closer) => Grow(i, j, 120);//grows quickly if nearby
    }
}