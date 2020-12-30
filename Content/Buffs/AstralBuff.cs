using Microsoft.Xna.Framework;

using Terraria;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Buffs
{
    class AstralBuff : SmartBuff
    {
        public AstralBuff() : base("Zapped!", "Losing life, but zapping nearby enemies!", true) { }

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/MarioCumming";
            return true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= 40;

            if (Main.rand.Next(10) == 0)
            {
                Vector2 pos = player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(player.width);
                Helper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(5, 10), DustType<Content.Dusts.Electric>(), 0.8f, 3);
            }

            if (Main.rand.Next(20) == 0)
            {
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC npc = Main.npc[k];
                    if (npc.active && Vector2.Distance(npc.Center, player.Center) < 100)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<LightningNode>(), 20, 0, player.whoAmI, 2, 100);
                        Helper.DrawElectricity(player.Center, npc.Center, DustType<Content.Dusts.Electric>());
                        return;
                    }
                }
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.Next(10) == 0)
            {
                Vector2 pos = npc.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(npc.width);
                Helper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(5, 10), DustType<Content.Dusts.Electric>(), 0.8f, 3);
            }

            if (Main.rand.Next(20) == 0)
            {
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC target = Main.npc[k];
                    if (target.active && Vector2.Distance(target.Center, npc.Center) < 100)
                    {
                        Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<LightningNode>(), 20, 0, 0, 2, 100);
                        Helper.DrawElectricity(npc.Center, target.Center, DustType<Content.Dusts.Electric>());
                        return;
                    }
                }
            }
        }
    }

    internal class LightningNode : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

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
                Helper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Content.Dusts.Electric>());
            }
            projectile.timeLeft = 0;
        }
    }
}
