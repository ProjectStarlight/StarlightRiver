using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class JungleHolyFountain : ModFountain
    {
        public JungleHolyFountain() : base("JungleHolyFountainItem", AssetDirectory.JungleHolyTile) { }
        public override void FountainActive(int i, int j, bool closer) =>
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleHoly = true;
    }

    internal class JungleHolyFountainItem : QuickTileItem
    {
        public JungleHolyFountainItem() : base("Hallowed Jungle Fountain", "Fruit salad.\nYummy yummy.", "JungleHolyFountain", texturePath: AssetDirectory.JungleHolyTile) { }
    }
}