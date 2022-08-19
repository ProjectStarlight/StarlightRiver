using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeDoor : ModTile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults()
        {
            (this).QuickSetFurniture(1, 1, DustID.Copper, SoundID.Shatter, false, Color.Black, false, true);
            MinPick = 999;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            tile.IsActuated = (!NPC.AnyNPCs(NPCType<Bosses.GlassMiniboss.Glassweaver>()));
        }
    }

    class ForgeDoorItem : QuickTileItem
    {
        public ForgeDoorItem() : base("Forge Door", "Debug Item", "ForgeDoor", 1, AssetDirectory.Debug, true) { }
    }
}
