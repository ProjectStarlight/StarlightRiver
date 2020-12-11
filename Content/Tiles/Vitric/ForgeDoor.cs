using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class ForgeDoor : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            QuickBlock.QuickSetFurniture(this, 2, 6, DustType<Content.Dusts.Air>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Forge Door");
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            tile.inActive(!NPC.AnyNPCs(NPCType<Bosses.GlassMiniboss.GlassMiniboss>()));
        }
    }

    class ForgeDoorItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public ForgeDoorItem() : base("Forge Door", "Titties", TileType<ForgeDoor>(), 1) { }
    }
}
