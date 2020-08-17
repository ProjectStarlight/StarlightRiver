using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Decoration
{
    class VitricFurniture : AutoFurniture
    {
        public VitricFurniture() : base("Vitric", "StarlightRiver/Tiles/Vitric/Decoration/", new Color(200, 150, 20), new Color(255, 220, 150), DustType<Dusts.Air>(), ItemType<Blocks.AncientSandstoneItem>()) { }
    }
}
