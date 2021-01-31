using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    internal partial class GlassMiniboss : ModNPC
    {
        Vector2 moveStart;
        Vector2 moveTarget;
        Player Target => Main.player[npc.target];

        private void ResetAttack() => AttackTimer = 0;

        private Vector2 PickSide() => Main.player[npc.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-532, 260) : spawnPos + new Vector2(532, 260); //picks the opposite side of the player.

        private Vector2 PickSideClose() => Main.player[npc.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-160, 60) : spawnPos + new Vector2(144, 60); //picks the same side of the player.

        private int Direction => npc.Center.X > spawnPos.X ? -1 : 1;

        private int DirectionAway => Direction * -1;

        private void SpawnAnimation()
        {
            if (AttackTimer == 1) moveStart = npc.Center;
            npc.Center = Vector2.SmoothStep(moveStart, spawnPos + new Vector2(0, -50), AttackTimer / 300f);
        }

        private void Idle(int duration)
        {
            npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;

            if (AttackTimer == 1)
                CurrentAnimation = AttackPhase > 6 ? AnimationSelection.WalkSword : AnimationSelection.Walk;

            Frame = ((int)AttackTimer / 5) % 10;

            npc.TargetClosest();
            npc.velocity.X += Target.Center.X > npc.Center.X ? 0.25f : -0.25f;

            if (System.Math.Abs(npc.velocity.X) >= 6)
                npc.velocity.X = npc.velocity.X > 0 ? 6 : -6;

            if (npc.collideX && npc.velocity.Y == 0) 
                npc.velocity.Y -= 10;

            if (AttackTimer == duration) 
                ResetAttack();
        }

        private void Hammer()
        {
            if(AttackTimer == 1)
                CurrentAnimation = AnimationSelection.SummonHammer;

            if (AttackTimer <= 40)
            {
                Frame = (int)(AttackTimer / 40f * 16);
                glowStrength = 0.25f + ((40 - AttackTimer) / 40f * 0.75f);
            }

            if (AttackTimer < 10) 
                npc.velocity *= 0.9f; //decelerate into position

            if (AttackTimer == 10) //stop and spawn projectile
            {
                npc.velocity *= 0;
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, npc.direction);
            }

            if (AttackTimer >= 90)
                ResetAttack();
        }

        private void Spears() //summon a wall of spears on one side of the arena
        {
            if (AttackTimer == 1)
            {
                npc.TargetClosest();
                moveTarget = PickSide();
                moveStart = npc.Center;
            }

            if (AttackTimer < 60) //go to the side away from the target
                npc.Center = Vector2.SmoothStep(moveStart, moveTarget, AttackTimer / 60f);

            if (AttackTimer == 90) //spawn the projectiles
            {
                int exclude = Main.rand.Next(3);
                for (int k = 0; k < 6; k++)
                {
                    if ((k / 2) != exclude) //leave an opening!
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, k * 60, Direction);
                }
            }

            if (AttackTimer >= 210)
                ResetAttack();
        }

        private void Knives() //Summon 3 knives that float up and home-in on the target. Target is passed to the knives as ai[0].
        {
            if (AttackTimer == 1)
            {
                for (int k = -1; k < 2; k++) //spawn projectiles
                    Projectile.NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(k) * -3, ProjectileType<GlassKnife>(), 20, 1, Main.myPlayer, npc.target);
            }

            if (AttackTimer == 60) 
                ResetAttack(); //TODO: May need to leave more time? unsure
        }

        private void Greatsword()
        {
            if (AttackTimer == 1)
            {

            }
        }

        private void UppercutGlide() //its just the other two methods put together because I re-use uppercut. Suck my cock.
        {
            if (AttackTimer == 1)
                CurrentAnimation = AnimationSelection.SummonSword;

            if (AttackTimer <= 40)
            {
                Frame = (int)((AttackTimer / 40f) * 12);
                glowStrength = 0.25f + ((40 - AttackTimer) / 40f * 0.75f);
            }

            if (AttackTimer <= 85) Uppercut(30);
            else if (AttackTimer <= 245) Helicopter(85);

            if (AttackTimer >= 260) ResetAttack();
        }

        private void SlashUpSlash() //slash up slash combo
        {
            if (AttackTimer == 1)
                CurrentAnimation = AnimationSelection.SummonSword;

            if (AttackTimer <= 40)
            {
                Frame = (int)((AttackTimer / 40f) * 12);
                glowStrength = 0.25f + ((40 - AttackTimer) / 40f * 0.75f);
            }

            if (AttackTimer <= 50) Slash(30);
            if (AttackTimer <= 80 && AttackTimer > 40) Uppercut(40);
            if (AttackTimer <= 120 && AttackTimer > 100) Slash(100);
            if (AttackTimer >= 150) ResetAttack();
        }

        private void Slash(int startTime)
        {
            if (AttackTimer == startTime + 1) //spawn projectile and set velcoity, along with targeting
            {
                npc.TargetClosest();
                npc.spriteDirection = npc.Center.X > Main.player[npc.target].Center.X ? 1 : -1;
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSlash>(), 34, 1, Main.myPlayer, npc.whoAmI, npc.spriteDirection);

                npc.velocity.X += npc.Center.X > Target.Center.X ? -12 : 12;
            }

            if (AttackTimer < startTime + 20) //decelerate
                npc.velocity.X *= 0.93f;

            if (AttackTimer == startTime + 20) //stop
                npc.velocity.X = 0;
        }

        private void Uppercut(int startTime)
        {
            if (AttackTimer == startTime + 30) //this is so fucking basic it just spawns a stupipd ass hitbox projectile and launches him up how fucking DUMB do you have to be to have to read this comment
            {
                npc.TargetClosest();
                npc.spriteDirection = npc.Center.X > Main.player[npc.target].Center.X ? 1 : -1;

                npc.velocity.Y = -14;
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassUppercut>(), 40, 1, Main.myPlayer, npc.whoAmI, npc.spriteDirection);
            }
        }

        private void Helicopter(int startTime) //roflcopter
        {
            if (AttackTimer == startTime + 1)
            {
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSpin>(), 30, 1, Main.myPlayer, npc.whoAmI); //spawn slash and let him fly through the air like a majestic dragon with a 17 INCH LONG COCK
                npc.noGravity = true;
                npc.TargetClosest();
            }

            npc.velocity.Y = 0.7f + Target.Center.Y > npc.Center.Y ? 1.2f : -0.6f; //still fall slowly

            if (AttackTimer <= startTime + 120)
            {
                npc.velocity.X += Target.Center.X > npc.Center.X ? 0.1f : -0.1f; //go towards the target
                if (npc.velocity.LengthSquared() > 36) //cap speed
                    npc.velocity = Vector2.Normalize(npc.velocity) * 6;
            }
            else
                npc.velocity.X *= 0.98f;

            if (AttackTimer < startTime + 150) //rotate in air
                spinAngle += 0.25f;

            if (AttackTimer == startTime + 150) //let him fall again after this shit is over
            {
                npc.noGravity = false;
                npc.velocity *= 0;
                spinAngle = 0;
            }
        }

        public static void SpawnShards(int amount, Vector2 pos)
        {
            for (int k = 0; k < amount; k++)
            {
                var spawnPos = pos + Vector2.One.RotatedByRandom(6.28f) * 10;
                var proj = Projectile.NewProjectile(spawnPos, Vector2.Zero, ProjectileType<ChargeShard>(), 0, 0, Main.myPlayer);

                var mp = Main.projectile[proj].modProjectile;

                if (mp is ChargeShard)
                    (mp as ChargeShard).target = pos.X > GlassMiniboss.spawnPos.X ? RightForge : LeftForge;
            }
        }
    }
}
