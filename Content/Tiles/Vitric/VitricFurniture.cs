using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class VitricFurniture : AutoFurniture
    {
        public VitricFurniture() : base("Vitric", "StarlightRiver/Assets/Tiles/Vitric/Decoration", new Color(140, 97, 86), new Color(255, 220, 150), DustType<Dusts.Air>(), ItemType<Blocks.AncientSandstoneItem>()) { }
    }
}
