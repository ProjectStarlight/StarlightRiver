using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleCorrupt
{
    public class VineJungleCorrupt : ModVine
    {
        public VineJungleCorrupt() : base(new string[] { "GrassJungleCorrupt" }, 14, new Color(64, 57, 94), 2, path: AssetDirectory.JungleCorruptTile) { }

        public override void NearbyEffects(int i, int j, bool closer) => Grow(i, j, 120);//grows quickly if nearby
    }
}