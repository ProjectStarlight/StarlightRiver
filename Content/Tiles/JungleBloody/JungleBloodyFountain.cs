using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class JungleBloodyFountain : ModFountain
    {
        public JungleBloodyFountain() : base("JungleBloodyFountainItem", AssetDirectory.JungleBloodyTile) { }
        public override void FountainActive(int i, int j, bool closer) =>
            Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleBloody = true;
    }

    internal class JungleBloodyFountainItem : QuickTileItem
    {
        public JungleBloodyFountainItem() : base("Crimson Jungle Fountain", "Fruit salad.\nYummy yummy.", "JungleBloodyFountain", texturePath: AssetDirectory.JungleBloodyTile) { }
    }
}