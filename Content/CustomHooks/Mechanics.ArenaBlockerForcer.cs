using Microsoft.Xna.Framework;
using Terraria;
using StarlightRiver.Content.Bosses.SquidBoss;
using Terraria.ModLoader;

using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
    class ArenaBlockerForcer : HookGroup {
        // i'm writing this as i've just woken up at like 10pm, excuse me if this is a bit badly written
        public override SafetyLevel Safety => SafetyLevel.Safe;
        public override void Load() {
            On.Terraria.Player.Update_NPCCollision += ForceColliding;
        }

        // for the most part vanilla
        private void ForceColliding(On.Terraria.Player.orig_Update_NPCCollision orig, Player self) {
            Rectangle rectangle = new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height);

            for (int i = 0; i < 200; i++) {
                if (!Main.npc[i].active || Main.npc[i].friendly || Main.npc[i].damage <= 0)
                    continue;

                var npc = Main.npc[i];
                int type = npc.type;
                Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                int specialHit = -1;
                float damageMultipler = 1;

                NPC.GetMeleeCollisionData(rectangle, i, ref specialHit, ref damageMultipler, ref npcRect);

                if (rectangle.Intersects(npcRect) && type == NPCType<ArenaBlocker>() && NPCLoader.CanHitPlayer(npc, self, ref specialHit)) {
                    NPCLoader.OnHitPlayer(npc, self, npc.damage, false);

                    // i fucking hate ref variables
                    var dam = npc.damage;
                    var crit = false;
                    NPCLoader.ModifyHitPlayer(npc, self, ref dam, ref crit);

                    return;
                }
            }

            orig(self);
        }
	}
}
