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

        private Vector2 PickSide() => Main.player[npc.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-160, 60) : spawnPos + new Vector2(144, 60); //picks the opposite side of the player.

        private Vector2 PickSideClose() => Main.player[npc.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-160, 60) : spawnPos + new Vector2(144, 60); //picks the same side of the player.

        private void SpawnAnimation()
        {
            if (AttackTimer == 1) moveStart = npc.Center;
            npc.Center = Vector2.SmoothStep(moveStart, spawnPos + new Vector2(0, -100), AttackTimer / 300f);
        }

        private void HammerSlam()
        {
            if (AttackTimer == 1) //sets appropriate movement points
            {
                npc.TargetClosest();
                moveTarget = PickSide();
                moveStart = npc.Center;
            }

            if (AttackTimer < 60) npc.Center = Vector2.SmoothStep(moveStart, moveTarget, AttackTimer / 60f); //move into position

            if (AttackTimer == 60)
            {
                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 0, Main.myPlayer, npc.Center.X > spawnPos.X ? -1 : 1); //spawn our hammer, see GlassHammer's AI for more information
                npc.direction = npc.Center.X > spawnPos.X ? -1 : 1;
                npc.spriteDirection = npc.direction;
            }

            if (AttackTimer >= 180) ResetAttack();
        }

        #region Slash Combo Attack
        private void SlashCombo()
        {
            if (GetRegion(npc) == RegionCenter) SlashComboCenter();
            else if (GetRegion(npc) == RegionLeft) SlashComboLeft();
            else if (GetRegion(npc) == RegionRight) SlashComboRight();
            else if (GetRegion(npc) == RegionPit) SlashComboPit();
        }

        private void SlashComboCenter()
        {
            if (AttackTimer == 1)
            {
                moveTarget = PickSideClose();
                moveStart = npc.Center;
            }

            if (AttackTimer < 60) npc.Center = Vector2.SmoothStep(moveStart, moveTarget, AttackTimer / 60f); //move into position

            if (AttackTimer >= 60 && AttackTimer % 30 == 0)
            {
                npc.velocity.X += moveTarget.X > spawnPos.X ? -6 : 6; //burst forward
                npc.direction = npc.velocity.X > 0 ? 1 : -1;

                int p = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSlash>(), 15, 1, Main.myPlayer, npc.direction == -1 ? 0 : 1, AttackTimer == 90 ? 1 : 0);
                (Main.projectile[p].modProjectile as GlassSlash).parent = this;


            }

            if (AttackTimer > 60) npc.velocity.X *= 0.95f; //decelerate

            if (AttackTimer >= 140)
            {
                npc.velocity *= 0;
                ResetAttack();
            }
        }

        private void SlashComboLeft()
        {
            if (AttackTimer == 1)
            {
                npc.TargetClosest();
                npc.velocity.X += Target.Center.X > npc.Center.X ? 3 : -3;

                int p = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSlash>(), 15, 1, Main.myPlayer, npc.direction == -1 ? 0 : 1, 0);
                (Main.projectile[p].modProjectile as GlassSlash).parent = this;
            }

            npc.velocity.X *= 0.96f;
            if(npc.Center.X > spawnPos.X - 280 && npc.velocity.X > 0) npc.velocity *= 0;

            if (AttackTimer >= 60)
            {
                npc.velocity *= 0;
                ResetAttack();
            }
        }

        private void SlashComboRight()
        {
            if (AttackTimer == 1)
            {
                npc.TargetClosest();
                npc.velocity.X += Target.Center.X > npc.Center.X ? 3 : -3;

                int p = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSlash>(), 15, 1, Main.myPlayer, npc.direction == -1 ? 0 : 1, 0);
                (Main.projectile[p].modProjectile as GlassSlash).parent = this;
            }

            npc.velocity.X *= 0.96f;
            if (npc.Center.X < spawnPos.X + 232 && npc.velocity.X < 0) npc.velocity *= 0;

            if (AttackTimer >= 60)
            {
                npc.velocity *= 0;
                ResetAttack();
            }
        }

        private void SlashComboPit()
        {
            if (AttackTimer == 1)
            {
                npc.TargetClosest();

                int p = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassSlash>(), 15, 1, Main.myPlayer, npc.direction == -1 ? 0 : 1, 1);
                (Main.projectile[p].modProjectile as GlassSlash).parent = this;

                npc.velocity.X += Target.Center.X > npc.Center.X ? 7 : -7;
            }

            if (AttackTimer < 60) npc.velocity.X *= 0.96f; //decelerate

            if (AttackTimer >= 60)
            {
                npc.velocity *= 0;
                ResetAttack();
            }
        }
        #endregion

        private void SummonKnives()
        {
            if (AttackTimer >= 60) ResetAttack();
        }
    }
}
