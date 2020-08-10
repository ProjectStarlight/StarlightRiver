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

            if (AttackTimer == 60) Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 0, Main.myPlayer, npc.Center.X > spawnPos.X ? -1 : 1); //spawn our hammer, see GlassHammer's AI for more information

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

            if (AttackTimer == 70) Projectile.NewProjectile(npc.Center + new Vector2(0, 32), Vector2.Zero, ProjectileType<GlassSlash>(), 40, 0.5f, Main.myPlayer, moveTarget.X > spawnPos.X ? 0 : 1); //spawn the slash

            if (AttackTimer > 60 && AttackTimer % 20 == 0 && AttackTimer <= 120) npc.velocity.X += moveTarget.X > spawnPos.X ? -5 : 5; //burst forward

            if (AttackTimer > 60) npc.velocity.X *= 0.95f; //decelerate

            if (AttackTimer >= 180)
            {
                npc.velocity *= 0;
                ResetAttack();
            }
        }

        private void SlashComboLeft()
        {
            ResetAttack();
        }

        private void SlashComboRight()
        {
            ResetAttack();
        }

        private void SlashComboPit()
        {
            ResetAttack();
        }
        #endregion

        private void SummonKnives()
        {
            if (AttackTimer == 1) npc.TargetClosest();

            if (AttackTimer >= 60 && AttackTimer % 30 == 0)
            {
                for(int k = 0; k < 3; k++)
                    Projectile.NewProjectile(npc.Center, Vector2.Normalize(npc.Center - Main.player[npc.target].Center).RotatedBy((k - 1) * 0.3f) * -1, ProjectileType<GlassKnife>(), 15, 0.2f, Main.myPlayer);

                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, npc.Center);
            }

            if (AttackTimer >= (Main.expertMode ? 160 : 120)) ResetAttack();
        }
    }
}
