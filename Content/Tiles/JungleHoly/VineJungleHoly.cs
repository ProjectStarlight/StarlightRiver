using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleHoly
{
    public class VineJungleHoly : ModVine
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleHolyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public VineJungleHoly() : base(new string[] { "GrassJungleHoly" }, 14, new Color(48, 141, 128), 2) { }

        public override void NearbyEffects(int i, int j, bool closer) => Grow(i, j, 120);//grows quickly if nearby
    }
}