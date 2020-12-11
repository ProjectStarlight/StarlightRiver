using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class VitricFurniture : FurnitureLoader
    {
        public VitricFurniture() : base("Vitric", "StarlightRiver/Assets/Tiles/Vitric/Decoration", new Color(140, 97, 86), new Color(255, 220, 150), DustType<Content.Dusts.Air>(), StarlightRiver.Instance.ItemType("AncientSandstoneItem")) { }
    }
}
