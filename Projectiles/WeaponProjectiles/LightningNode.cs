using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class LightningNode : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.timeLeft = 1;
            projectile.friendly = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //AI Fields:
            //0: jumps remaining
            //1: jump radius

            List<NPC> possibleTargets = new List<NPC>();
            foreach (NPC npc in Main.npc.Where(npc => npc.active && !npc.immortal && Vector2.Distance(npc.Center, projectile.Center) < projectile.ai[1] && npc != target))
            {
                possibleTargets.Add(npc); //This grabs all possible targets, which includes all NPCs in the appropriate raidus which are alive and vulnerable, excluding the hit NPC
            }
            if (possibleTargets.Count == 0) return; //kill if no targets are available
            NPC chosenTarget = possibleTargets[Main.rand.Next(possibleTargets.Count)];

            if (projectile.ai[0] > 0 && chosenTarget != null) //spawns the next node and VFX if more nodes are available and a target is also available
            {
                Projectile.NewProjectile(chosenTarget.Center, Vector2.Zero, ProjectileType<LightningNode>(), damage, knockback, projectile.owner, projectile.ai[0] - 1, projectile.ai[1]);
                Helper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Dusts.Electric>());
            }
            projectile.timeLeft = 0;
        }
    }
}