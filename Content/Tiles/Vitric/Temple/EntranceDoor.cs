using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class EntranceDoor : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

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
}
