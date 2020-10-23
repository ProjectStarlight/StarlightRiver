using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;
using StarlightRiver.Items;

namespace StarlightRiver.Tiles.Vitric
{
    class ForgeDoor : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            QuickBlock.QuickSetFurniture(this, 2, 6, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Forge Door");
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            tile.inActive(!NPC.AnyNPCs(NPCType<NPCs.Miniboss.Glassweaver.GlassMiniboss>()));
        }
    }

    class ForgeDoorItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public ForgeDoorItem() : base("Forge Door", "Titties", TileType<ForgeDoor>(), 1) { }
    }
}
