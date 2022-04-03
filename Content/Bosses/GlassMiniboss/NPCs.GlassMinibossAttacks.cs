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
        Player Target => Main.player[NPC.target];

        private void ResetAttack() => AttackTimer = 0;

        private Vector2 PickSide() => Main.player[NPC.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-532, 260) : spawnPos + new Vector2(532, 260); //picks the opposite side of the Player.

        private Vector2 PickSideClose() => Main.player[NPC.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-160, 60) : spawnPos + new Vector2(144, 60); //picks the same side of the Player.

        private int Direction => NPC.Center.X > spawnPos.X ? -1 : 1;

        private int DirectionAway => Direction * -1;

        private void SpawnAnimation()
        {
            if(AttackTimer < 60)
			{
                NPC.position.Y -= AttackTimer * 0.5f;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
			}

            if (AttackTimer == 60)
            {
                moveStart = NPC.Center;
                NPC.scale = 0.65f;
            }

            if(AttackTimer > 60 && AttackTimer < 90)
			{
                NPC.Center = Vector2.Lerp(moveStart, spawnPos + new Vector2(0, -60), (AttackTimer - 60) / 30f);
			}

            if (AttackTimer == 90)
            {
                Main.LocalPlayer.GetModPlayer<Core.StarlightPlayer>().Shake += 20;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
            }
        }

        private void Idle(int duration)
        {
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;

            Frame = ((int)AttackTimer / 5) % 10;

            NPC.TargetClosest();
            NPC.velocity.X += Target.Center.X > NPC.Center.X ? 0.25f : -0.25f;

            if (System.Math.Abs(NPC.velocity.X) >= 6)
                NPC.velocity.X = NPC.velocity.X > 0 ? 6 : -6;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y -= 10;

            if (AttackTimer == duration)
                ResetAttack();
        }

        private void Knives()
		{
            if (AttackTimer == 1)
                NPC.TargetClosest();

            if (AttackTimer == 5)
            {
                for (int k = 0; k < 9; k++)
                {
                    Projectile.NewProjectile(NPC.Center + new Vector2(0, -120), Vector2.Zero, ProjectileType<GlassKnife>(), 15, 1, Main.myPlayer, NPC.target, (k - 7) * 0.05f);
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
                NPC.velocity *= 0.8f; //decelerate into position

            if (AttackTimer == 30) //stop and spawn Projectile
            {
                NPC.velocity *= 0;
                Projectile.NewProjectile(NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.direction, NPC.whoAmI);
            }

            if (AttackTimer >= 110)
                ResetAttack();
        }

        private void Spears() //summon a wall of spears on one side of the arena, tentative keep?
        {
            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickSide();
                moveStart = NPC.Center;
            }

            if (AttackTimer < 60) //go to the side away from the target
                NPC.Center = Vector2.SmoothStep(moveStart, moveTarget, AttackTimer / 60f);

            if (AttackTimer == 90) //spawn the Projectiles
            {
                int exclude = Main.rand.Next(3);
                for (int k = 0; k < 6; k++)
                {
                    if ((k / 2) != exclude) //leave an opening!
                        Projectile.NewProjectile(NPC.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, k * 60, Direction);
                }
            }

            if (AttackTimer >= 210)
                ResetAttack();
        }
    }
}
