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
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleCorruptTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public VineJungleCorrupt() : base(new string[] { "GrassJungleCorrupt" }, 14, new Color(64, 57, 94), 2) { }

        public override void NearbyEffects(int i, int j, bool closer) => Grow(i, j, 120);//grows quickly if nearby
    }
}