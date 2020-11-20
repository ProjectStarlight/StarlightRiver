using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public class WispSuck : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.damage = 10;
            projectile.friendly = true;
            projectile.penetrate = 6;
            projectile.timeLeft = 280;
            projectile.magic = true;
            projectile.timeLeft = 8;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The unresistable sucction");
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[projectile.owner];

            Vector2 distance = player.Center - target.Center;

            target.velocity += Vector2.Normalize(distance) * -10;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            for (int k = 0; k <= 200; k++)
            {
                if (Main.npc[k].type == mod.NPCType("DesertWisp") || Main.npc[k].type == mod.NPCType("DesertWisp2"))
                {
                    if (Vector2.Distance(projectile.Center, Main.npc[k].position) <= 40)
                    {
                        Main.npc[k].localAI[2] += 5;
                        Main.npc[k].velocity = (Main.npc[k].position - player.Center).SafeNormalize(Vector2.Zero) * -4f;
                    }
                    if (Vector2.Distance(player.Center, Main.npc[k].position) <= 20)
                    {
                        if (Main.npc[k].active)
                        {
                            Helper.Kill(Main.npc[k]);
                            player.QuickSpawnItem(mod.ItemType("Wisp"));
                        }
                    }
                }
            }
            if (projectile.ai[1] == 0)
            {
                for (int counter = 0; counter <= 3; counter++)
                {
                    int dustType = 16;
                    Vector2 dustPos = projectile.Center;
                    Dust dust = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType, 0f, 0f, 100, default, 1f)];
                    dust.velocity = (dustPos - player.Center).SafeNormalize(Vector2.Zero) * -9f;
                    dust.noGravity = true;
                    dust.scale = 1.1f;
                    dust.noLight = true;
                }
            }
        }
    }
}