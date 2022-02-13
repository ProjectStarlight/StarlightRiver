using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricLargeFountain : ModFountain
    {
        public VitricLargeFountain() : base("VitricLargeFountainItem", AssetDirectory.VitricTile, 4, ModContent.DustType<Dusts.Air>(), new Color(140, 97, 86), 4, 6) { }

        public override void FountainActive(int i, int j, bool closer) =>
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainVitric = true;
    }
}