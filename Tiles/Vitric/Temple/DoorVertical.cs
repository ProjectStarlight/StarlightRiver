using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Microsoft.Xna.Framework;
using StarlightRiver.Items;

namespace StarlightRiver.Tiles.Vitric.Temple
{
    class DoorVertical : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
    }

    class DoorVerticalItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public DoorVerticalItem() : base("Vertical Temple Door", "CUM IN ME DADDY OH YES YES YES", TileType<DoorVertical>(), ItemRarityID.Blue) { }
    }
}
