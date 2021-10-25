using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricFountain : ModFountain
    {
        public VitricFountain() : base("VitricFountainItem", AssetDirectory.VitricTile) { }

        public override void FountainActive(int i, int j, bool closer) =>
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainVitric = true;
    }
}