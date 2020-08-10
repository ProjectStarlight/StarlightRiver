using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles
{
    public class FlameboltChild : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flamebolt");
        }

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.friendly = true;
            projectile.aiStyle = -1;
            projectile.magic = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 120;
        }

        private bool picked = false;
        private bool cameBack = false;
        private NPC target = Main.npc[0];
        private NPC blacklist;

        public override bool? CanHitNPC(NPC target)
        {
            if (cameBack && target.Equals(blacklist)) //if its original target is dead, only allow damage towards blacklist
            {
                return base.CanHitNPC(target);
            }
            return !cameBack && !target.Equals(blacklist) ? base.CanHitNPC(target) : false;
        }

        public void Pick()
        {
            for (int k = 0; k < Main.npc.Length; k++)
            {
                if (Vector2.Distance(Main.npc[k].Center, projectile.Center) < Vector2.Distance(target.Center, projectile.Center) && Main.npc[k].active && !Main.npc[k].friendly)
                {
                    if (!Main.npc[k].Equals(blacklist))
                    {
                        target = Main.npc[k];
                        picked = true;
                    }
                }
            }
        }

        //when spawned, choose a target that isnt the npc hit by the parent
        //home in on that target
        //if that target is dead
        //the next target is the npc hit by the parent
        public override void AI()
        {
            if (!picked)
            {
                blacklist = Main.npc[(int)projectile.ai[0]];
                Pick();
            }
            if (target == Main.npc[0])
            {
                projectile.velocity *= 0.99f;
                float rot = projectile.velocity.ToRotation();
                float x = projectile.velocity.X + (float)Math.Sin(rot) * ((float)Math.Sin((projectile.timeLeft + projectile.ai[1]) / 8) * 6);
                float y = projectile.velocity.Y + (float)Math.Cos(rot) * ((float)Math.Sin((projectile.timeLeft + projectile.ai[1]) / 8) * -6);
                projectile.velocity = new Vector2(x, y);
                if (projectile.timeLeft <= 60)
                {
                    target = blacklist;
                    cameBack = true;
                }
            }
            if (target != Main.npc[0])
            {
                projectile.velocity += Vector2.Normalize(target.Center - projectile.Center) * 3f;
                float rot = projectile.velocity.ToRotation();
                float x = projectile.velocity.X + (float)Math.Sin(rot) * ((float)Math.Sin((projectile.timeLeft + projectile.ai[1]) / 8) * 6);
                float y = projectile.velocity.Y + (float)Math.Cos(rot) * ((float)Math.Sin((projectile.timeLeft + projectile.ai[1]) / 8) * -6);
                projectile.velocity = new Vector2(x, y);
                projectile.velocity = Vector2.Normalize(projectile.velocity) * 12;
            }
            if (!target.active && blacklist.active && !cameBack)
            {
                target = blacklist;
                cameBack = true;
            }
            for (float f = 0; f <= 1f; f += 0.25f)
            {
                Vector2 dustPos = projectile.oldPosition + projectile.velocity * f;
                int dust = Dust.NewDust(dustPos, 0, 0, DustType<Gold2>(), 0, 0, 0, default, 1f);
                Main.dust[dust].position = dustPos;
                Main.dust[dust].velocity = Vector2.Zero;
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int num2 = 0; num2 < 7; num2++)
            {
                int num = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustType<Gold2>(), 0f, 0f, 25, default, 1.2f);
                Main.dust[num].velocity *= 2f;
            }
        }
    }
}