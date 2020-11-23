using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    class PalecrusherProj : ClubProj
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pale Crusher");
            Main.projFrames[projectile.type] = 2;
        }
        int[] targets = new int[3];
        public override void Smash(Vector2 position)
        {
            Player player = Main.player[projectile.owner];
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(projectile.oldPosition + new Vector2(projectile.width / 2, projectile.height / 2), DustType<Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * projectile.ai[0] / 10f);
            }
            for (int k = 0; k < 3; k++)
            {
                int range = 40;
                int target = -1;
                float lowestDist = float.MaxValue;
                for (int i = 0; i < 200; ++i)
                {
                    bool match = false;
                    NPC npc = Main.npc[i];
                    foreach (int j in targets)
                    {
                        if (j == i)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (npc.active && npc.CanBeChasedBy(projectile) && !npc.friendly && !match && !npc.noGravity)
                    {
                        float dist = projectile.Distance(npc.Center);
                        if (dist / 16 < range)
                        {
                            if (dist < lowestDist)
                            {
                                lowestDist = dist;

                                target = npc.whoAmI;
                                projectile.netUpdate = true;
                            }
                        }
                    }
                }
                targets[k] = target;
                if (targets[k] != -1)
                {
                    NPC npc = Main.npc[targets[k]];
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y - 32, 0, 0, ModContent.ProjectileType<PalePillar>(), projectile.damage / 3, projectile.knockBack / 2, projectile.owner);
                }
            }
            for (int j = 0; j < 3; j++)
            {
                if (targets[j] != -1)
                {
                    NPC npc = Main.npc[targets[j]];
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y - 32, 0, 0, ModContent.ProjectileType<PalePillar>(), projectile.damage / 3, projectile.knockBack / 2, projectile.owner);
                }
            }
        }
        public PalecrusherProj() : base(52, 16, 40, -1, 48, 4, 8, 2.1f, 18f) { }
    }
}
