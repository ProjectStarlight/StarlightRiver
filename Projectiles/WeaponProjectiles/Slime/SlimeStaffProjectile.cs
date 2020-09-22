using Microsoft.Xna.Framework;
using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles.Slime
{
    internal class SlimeStaffProjectile : ModProjectile
    {
        private float maxProjSpeed;
        public override void SetDefaults()
        {
            maxProjSpeed = 7.5f;
            projectile.width = 20;
            projectile.height = 20;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 60;
            projectile.timeLeft = 10240;
            projectile.aiStyle = -1;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime");
        }

        public override void AI()
        {
            Vector2 DirectionToCursor = new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center;
            projectile.velocity += Vector2.Normalize(DirectionToCursor) * 0.5f;//last float is how fast it turns


            projectile.velocity = projectile.velocity.Length() > maxProjSpeed ? //dont question it
                Vector2.Normalize(projectile.velocity) * maxProjSpeed : //Case 1A
                projectile.velocity.Length() < maxProjSpeed ?           //Case 1B
                    projectile.velocity * 1.01f :                       //Case 2A
                    projectile.velocity;                                //Case 2B

            if (Main.myPlayer == projectile.owner)
            {
                int currentTargetX = (int)(Main.MouseWorld.X * 10);//multiply or divide these to change precision, this seems to be enogh for multiplayer
                int oldTargetX = (int)(projectile.ai[0] * 10);//dividing by ten is the lowest you can go and avoid desyncs
                int currentTargetY = (int)(Main.MouseWorld.Y * 10);
                int oldTargetY = (int)(projectile.ai[1] * 10);

                projectile.ai[0] = Main.MouseWorld.X;
                projectile.ai[1] = Main.MouseWorld.Y;

                // This code checks if the precious velocity of the projectile is different enough from its new velocity, and if it is, syncs it with the server and the other clients in MP.
                // We previously multiplied the speed by 1000, then casted it to int, this is to reduce its precision and prevent the speed from being synced too much.
                if (currentTargetX != oldTargetX || currentTargetY != oldTargetY)
                {
                    projectile.netUpdate = true;
                    //Main.NewText("update " + projectile.velocity.Length());
                }
            }

            //Main.NewText(projectile.velocity.Length());

            //vfx
            projectile.rotation += 0.1f;
            Dust.NewDustPerfect(projectile.Center, 264, Vector2.Zero, 0, Color.BlueViolet, 0.4f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.penetrate--;
            if (projectile.penetrate <= 0)
            {
                projectile.Kill();
            }
            else
            {
                if (projectile.velocity.X != oldVelocity.X)
                {
                    projectile.velocity.X = -oldVelocity.X;
                }
                if (projectile.velocity.Y != oldVelocity.Y);
                {
                    projectile.velocity.Y = -oldVelocity.Y;
                }
                projectile.velocity = projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 0.75f;
                Main.PlaySound(SoundID.Item10, projectile.position);
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k <= 30; k++)
            {
                Color color = new Color(projectile.ai[0] / 200f, (300 - projectile.ai[0]) / 255f, (300 - projectile.ai[0]) / 255f);
                Dust.NewDustPerfect(projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f), 0, color, 0.8f);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }
    }
}