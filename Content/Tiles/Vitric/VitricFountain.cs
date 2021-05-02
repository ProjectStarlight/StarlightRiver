using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricFountain : ModFountain
    {
        public VitricFountain() : base("VitricFountainItem", AssetDirectory.VitricTile) { }
        public override void FountainActive(int i, int j, bool closer) =>
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainVitric = true;
    }

    internal class VitricFountainItem : QuickTileItem
    {
        public VitricFountainItem() : base("Vitric Fountain", "Fruit salad.\nYummy yummy.", "VitricFountain", ItemRarityID.White, AssetDirectory.VitricTile) { }
    }
}