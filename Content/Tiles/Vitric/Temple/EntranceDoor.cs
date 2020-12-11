using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class EntranceDoor : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(2, 7, DustType<Content.Dusts.Air>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Vitric Temple Door");
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (StarlightWorld.HasFlag(WorldFlags.DesertOpen)) tile.inActive(true);
            else tile.inActive(false);
        }
    }

    class EntranceDoorItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public EntranceDoorItem() : base("EntranceDoor", "Titties", TileType<EntranceDoor>(), 1) { }
    }
}
