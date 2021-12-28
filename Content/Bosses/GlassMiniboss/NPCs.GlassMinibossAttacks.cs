using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            if(AttackTimer < 60)
			{
                npc.position.Y -= AttackTimer * 0.5f;
                npc.noGravity = true;
                npc.noTileCollide = true;
			}

            if (AttackTimer == 60)
            {
                moveStart = npc.Center;
                npc.scale = 0.65f;
            }

            if(AttackTimer > 60 && AttackTimer < 90)
			{
                npc.Center = Vector2.Lerp(moveStart, spawnPos + new Vector2(0, -60), (AttackTimer - 60) / 30f);
			}

            if (AttackTimer == 90)
            {
                Main.LocalPlayer.GetModPlayer<Core.StarlightPlayer>().Shake += 20;
                npc.noGravity = false;
                npc.noTileCollide = false;
            }
        }

        private void Idle(int duration)
        {
            npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;

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

        private void Knives()
		{
            if (AttackTimer == 1)
                npc.TargetClosest();

            if (AttackTimer == 5)
            {
                for (int k = 0; k < 9; k++)
                {
                    Projectile.NewProjectile(npc.Center + new Vector2(0, -120), Vector2.Zero, ProjectileType<GlassKnife>(), 15, 1, Main.myPlayer, npc.target, (k - 7) * 0.05f);
                }
            }

            if (AttackTimer >= 90)
                ResetAttack();
		}

        private void Hammer()
        {
            if (AttackTimer <= 60)
            {
                glowStrength = 0.25f + ((60 - AttackTimer) / 60f * 0.75f);
            }

            if (AttackTimer < 10) 
                npc.velocity *= 0.8f; //decelerate into position

            if (AttackTimer == 30) //stop and spawn projectile
            {
                npc.velocity *= 0;
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, npc.direction, npc.whoAmI);
            }

            if (AttackTimer >= 110)
                ResetAttack();
        }

        private void Spears() //summon a wall of spears on one side of the arena, tentative keep?
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
    }
}
