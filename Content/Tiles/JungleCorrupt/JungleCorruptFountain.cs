using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class JungleCorruptFountain : ModFountain
    {
        public JungleCorruptFountain() : base("JungleCorruptFountainItem", AssetDirectory.JungleCorruptTile) { }
        public override void FountainActive(int i, int j, bool closer) =>
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleCorrupt = true;
    }

    internal class JungleCorruptFountainItem : QuickTileItem
    {
        public JungleCorruptFountainItem() : base("Corrupt Jungle Fountain", "Fruit salad.\nYummy yummy.", "JungleCorruptFountain", texturePath: AssetDirectory.JungleCorruptTile) { }
    }
}