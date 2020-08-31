using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    internal partial class GlassMiniboss : ModNPC
    {
        Vector2 moveStart;
        Vector2 moveTarget;
        Player Target => Main.player[npc.target];

        private void ResetAttack() => AttackTimer = 0;

        private Vector2 PickSide() => Main.player[npc.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-532, 60) : spawnPos + new Vector2(532, 60); //picks the opposite side of the player.

        private Vector2 PickSideClose() => Main.player[npc.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-160, 60) : spawnPos + new Vector2(144, 60); //picks the same side of the player.

        private int Direction => npc.Center.X > spawnPos.X ? -1 : 1;

        private int DirectionAway => Direction * -1;

        private void SpawnAnimation()
        {
            if (AttackTimer == 1) moveStart = npc.Center;
            npc.Center = Vector2.SmoothStep(moveStart, spawnPos + new Vector2(0, -50), AttackTimer / 300f);
        }

        private void Hammer()
        {
            if (AttackTimer < 10) npc.velocity *= 0.9f; //decelerate into position

            if(AttackTimer == 10) //stop and spawn projectile
            {
                npc.velocity *= 0;
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, npc.direction);
            }
        }

        private void Spears() //summon a wall of spears on one side of the arena
        {
            if(AttackTimer == 1)
            {
                npc.TargetClosest();
                moveTarget = PickSide();
                moveStart = npc.Center;
            }

            if (AttackTimer < 60) //go to the side away from the target
                npc.Center = Vector2.SmoothStep(moveStart, moveTarget, AttackTimer / 60f);

            if(AttackTimer == 90) //spawn the projectiles
            {
                int exclude = Main.rand.Next(3);
                for(int k = 0; k < 6; k++)
                {
                    if((k / 2) != exclude) //leave an opening!
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 20, 1, Main.myPlayer, k * 60, Direction);
                }
            }

            if (AttackTimer >= 240) ResetAttack();
        }

        private void Knives() //Summon 3 knives that float up and home-in on the target. Target is passed to the knives as ai[0].
        {
            if (AttackTimer == 1)
            {
                for (int k = -1; k < 2; k++) //spawn projectiles
                    Projectile.NewProjectile(npc.Center, Vector2.UnitY.RotatedBy(k) * -3, ProjectileType<GlassKnife>(), 15, 1, Main.myPlayer, npc.target);
            }

            if (AttackTimer == 60) ResetAttack(); //TODO: May need to leave more time? unsure
        }

        private void Greatsword()
        {

        }
    }
}
