using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricVine : ModVine
    {
        public VitricVine() : base(new string[] { "VitricSand" }, DustType<Dusts.Air>(), new Color(199, 224, 190), path: AssetDirectory.VitricTile) { }
    }
}