using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricBossBarrier : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            TileID.Sets.DrawsWalls[Type] = true;
            minPick = 999;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Main.tileSolid[Type] = Main.npc.Any(n => n.active && n.type == NPCType<NPCs.Boss.VitricBoss.VitricBoss>());
        }
    }
}