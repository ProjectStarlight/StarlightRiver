using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class GlassMiniboss : ModNPC
    {
        Vector2 moveStart;
        Vector2 moveTarget;
        Player Target => Main.player[NPC.target];

        private void ResetAttack() => AttackTimer = 0;

        private Vector2 PickSide(int x = 1) => Main.player[NPC.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-520 * x, 240) : spawnPos + new Vector2(520 * x, 240); //picks the outer side.

        private Vector2 PickSideClose(int x = 1) => Main.player[NPC.target].Center.X > spawnPos.X ? spawnPos + new Vector2(-160 * x, 40) : spawnPos + new Vector2(144 * x, 40); //picks the inner side.

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
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;

            NPC.TargetClosest();
            NPC.velocity.X += Target.Center.X > NPC.Center.X ? 0.25f : -0.25f;

            if (System.Math.Abs(NPC.velocity.X) >= 6)
                NPC.velocity.X = NPC.velocity.X > 0 ? 6 : -6;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y -= 10;

            if (AttackTimer == duration)
                ResetAttack();
        }

        private void JumpToTarget(float progress, int timerStart = 1)
        {
            if (AttackTimer < timerStart)
                moveStart = NPC.position;

            NPC.velocity.Y = progress * (NPC.Distance(moveTarget) / 20f);
            NPC.position.X = MathHelper.Lerp(moveStart.X, moveTarget.X, Helpers.Helper.BezierEase(progress)) + (NPC.width / 2f);
        }

        private void Spears()
        {
            AttackType = (int)AttackEnum.Spears;

            int spearCount = 12;

            NPC.TargetClosest();
            NPC.FaceTarget();

            if (AttackTimer == 1)
            {
                moveStart = NPC.Center;
                moveTarget = PickSideClose() + new Vector2(0, 50);
            }

            if (AttackTimer > 1 && AttackTimer < 20)
                NPC.velocity.Y = Helpers.Helper.BezierEase((20 - (AttackTimer - 5)) / 20f) * -0.02f * Math.Max(NPC.Distance(moveTarget), 3f);

            moveTarget = new Vector2(MathHelper.Lerp(moveTarget.X, Target.Center.X, 0.005f), moveTarget.Y);

            if (AttackTimer > 10)
            {
                NPC.noGravity = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * Math.Max(NPC.Distance(moveTarget), 3f), 0.03f) * 0.2f * Utils.GetLerpValue(10, 20, AttackTimer, true);
                NPC.Center += new Vector2((float)Math.Sin(AttackTimer * 0.04f % MathHelper.TwoPi), (float)Math.Cos(AttackTimer * 0.04f % MathHelper.TwoPi)) * 0.2f;
            }
            const int spearStart = 90;

            if (AttackTimer % 5 == 0 && AttackTimer >= spearStart && AttackTimer < spearStart + (spearCount * 5f))
            {
                Vector2 staffPos = NPC.Center + new Vector2(28 * NPC.direction, -88).RotatedBy(NPC.rotation);
                Vector2 spearTarget = PickSideClose(-1) + new Vector2(Main.rand.Next(-200, 100) * NPC.direction, 50);
                Vector2 spearVel = Main.rand.NextVector2CircularEdge(4, 4) + Main.rand.NextVector2Circular(4, 4);
                Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), staffPos, spearVel, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, staffPos.AngleTo(spearTarget));
            }

            if (AttackTimer >= 350)
                ResetAttack();
        }

        private const int hammerSpawn = 90;

        private void Hammer()
        {
            AttackType = (int)AttackEnum.Hammer;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickSide(-1);
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer < 50)
            {
                JumpToTarget(Utils.GetLerpValue(45, 2, AttackTimer, true));
                NPC.FaceTarget();
            }

            if (!(AttackTimer > 1 && AttackTimer < 70))
                NPC.velocity.X *= 0.8f;

            if (AttackTimer == hammerSpawn)
                Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.whoAmI);

            if (AttackTimer > 250)
                ResetAttack();

        }

        private void BigBrightBubble()
        {
            AttackType = (int)AttackEnum.BigBrightBubble;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = spawnPos + new Vector2(0, -80);
                moveStart = NPC.Center;
            }

            if (AttackTimer < 50)
                NPC.direction = NPC.velocity.X < 0 ? -1 : 1;
            else if (AttackTimer == 50)
            {
                NPC.FaceTarget();
                Vector2 staffPos = NPC.Center + new Vector2(28 * NPC.direction, -88).RotatedBy(NPC.rotation);
                Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), staffPos, Vector2.Zero, ProjectileType<GlassSpear>(), 50, 2f, Main.myPlayer);
            }
        }
    }
}
