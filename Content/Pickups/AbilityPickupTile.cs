using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Pickups
{
	public abstract class AbilityPickupTile : ModTile
    {
        public virtual int PickupType => 0;

        private int spawnAttemptTimer = 0;
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 1, 1, 0, 0, false, Color.White);
            //minPick = int.MaxValue;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (spawnAttemptTimer > 0)
            {
                spawnAttemptTimer--;
                return;
            }
            spawnAttemptTimer = 60;

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC npc = Main.npc[k];
                if (npc.active && npc.type == PickupType && Vector2.DistanceSquared(npc.position, new Vector2(i, j) * 16) <= 128)
                    return;
            }

            Packets.SpawnNPC abilitySpawnPacket = new Packets.SpawnNPC(Main.myPlayer, i * 16 + 8, j * 16 + 24, PickupType);
            abilitySpawnPacket.Send();
        }
    }
}
