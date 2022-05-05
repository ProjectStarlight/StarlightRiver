using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
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

        //according to the targets position,
        private Vector2 PickSide(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-510 * x, 40) : arenaPos + new Vector2(510 * x, 40); //picks the outer side.

        private Vector2 PickCloseSide(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-140 * x, 0) : arenaPos + new Vector2(140 * x, 0); //picks the inner side.

        private Vector2 PickSideFromMe(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(510 * x, 40) : arenaPos + new Vector2(-510 * x, 40); //picks the outer side.

        private Vector2 PickCloseSideNearMe(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(140 * x, 0) : arenaPos + new Vector2(-140 * x, 0); //picks the inner side.

        private int Direction => NPC.Center.X > arenaPos.X ? -1 : 1;

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
                NPC.Center = Vector2.Lerp(moveStart, arenaPos + new Vector2(0, -60), (AttackTimer - 60) / 30f);
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

            if (Math.Abs(NPC.velocity.X) >= 6)
                NPC.velocity.X = NPC.velocity.X > 0 ? 6 : -6;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y -= 10;

            if (AttackTimer == duration)
                ResetAttack();
        }

        private void JumpToTarget(int timeStart, int timeEnd, float yStrength = 1f)
        {
            float progress = Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true);

            if (AttackTimer < timeStart)
                moveStart = NPC.position;
            if (AttackTimer == timeStart + 1)
                NPC.velocity.Y = -(10f + (moveStart.Y - moveTarget.Y)) * yStrength;

            if (progress <= 0.6f)
            {
                NPC.velocity.X = (moveStart.X - moveTarget.X) * -0.015f;
                moveStart.X += NPC.velocity.X * 0.15f;
            }
            else
                NPC.velocity.Y += progress * progress * 0.7f;

            NPC.position.X = MathHelper.SmoothStep(moveStart.X, moveTarget.X, progress) - (NPC.width / 2f);
        }

        private void SpinJumpToTarget(int timeStart, int timeEnd, float totalRotations = 5, int direction = 1)
        {
            JumpToTarget(timeStart, timeEnd, 0.9f);
            float progress = Helpers.Helper.BezierEase(Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true)); 
            NPC.rotation = MathHelper.WrapAngle(progress * MathHelper.TwoPi * totalRotations) * NPC.direction * direction;
        }

        private int spearTime;
        private int hammerTime;
        private const int spearSpawn = 90;
        private const int hammerSpawn = 90;
        private const int bubbleRecoil = 450;

        public int hammerIndex;
        public int bubbleIndex;

        private void Spears()
        {
            AttackType = (int)AttackEnum.Spears;

            int spearCount = 12;
            int betweenSpearTime = 8;

            if (Main.masterMode)
            {
                spearCount = 18;
                betweenSpearTime = 4;
            }

            spearTime = 210 + spearSpawn + (spearCount * betweenSpearTime);

            NPC.TargetClosest();
            NPC.FaceTarget();

            if (AttackTimer == 1)
            {
                moveStart = NPC.Center;
                moveTarget = PickCloseSide() - new Vector2(0, 80);
            }

            if (AttackTimer > 1 && AttackTimer < 20)
                NPC.velocity.Y = -Helpers.Helper.SwoopEase(Utils.GetLerpValue(0, 15, AttackTimer, true)) * 0.05f * (NPC.Center.X - moveTarget.X) / 30f;

            moveTarget.X = MathHelper.Lerp(moveTarget.X, Target.Center.X, 0.007f);

            if (AttackTimer > 10 && AttackTimer < 210 + (spearCount * betweenSpearTime))
            {
                NPC.noGravity = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * Math.Max(NPC.Distance(moveTarget), 3f), 0.03f) * 0.2f * Utils.GetLerpValue(10, 30, AttackTimer, true);
                NPC.Center += new Vector2((float)Math.Sin(AttackTimer * 0.04f % MathHelper.TwoPi), (float)Math.Cos(AttackTimer * 0.04f % MathHelper.TwoPi)) * 0.2f;
            }
            else
                NPC.velocity.X *= 0.8f;

            if (AttackTimer % betweenSpearTime == 0 && AttackTimer >= spearSpawn && AttackTimer < spearSpawn + (spearCount * betweenSpearTime))
            {
                Vector2 staffPos = NPC.Center + new Vector2(28 * NPC.direction, -92).RotatedBy(NPC.rotation);
                Vector2 spearTarget = PickSide(-1) + new Vector2(Main.rand.Next(-200, 20) * NPC.direction, 100);
                Vector2 spearVel = Main.rand.NextVector2CircularEdge(4, 4) + Main.rand.NextVector2Circular(4, 4);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, spearVel, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, staffPos.AngleTo(spearTarget));
            }

            if (AttackTimer > spearTime)
                ResetAttack();
        }

        private void Hammer()
        {
            AttackType = (int)AttackEnum.Hammer;
            hammerTime = 90;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickSide(-1);
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer < 80)
            {
                JumpToTarget(5, 75);
                NPC.direction = Direction;
            }

            if (!(AttackTimer > 70 && AttackTimer < 100))
                NPC.velocity.X *= 0.9f;

            if (AttackTimer == hammerSpawn)
                hammerIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.whoAmI, hammerTime);

            if (AttackTimer > 180 + hammerTime)
                ResetAttack();

        }

        //spikes out from center instead of side
        private void HammerVariant()
        {
            AttackType = (int)AttackEnum.Hammer;
            hammerTime = 120;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickCloseSideNearMe();
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer < 80)
            {
                JumpToTarget(5, 75);
                NPC.direction = Direction;
            }

            if (!(AttackTimer > 70 && AttackTimer < 100))
                NPC.velocity.X *= 0.9f;

            if (AttackTimer == hammerSpawn)
                hammerIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.whoAmI, hammerTime);

            if (AttackTimer > 180 + hammerTime)
                ResetAttack();

        }

        private void BigBrightBubble()
        {
            AttackType = (int)AttackEnum.BigBrightBubble;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveStart = NPC.Center;
                moveTarget = arenaPos - new Vector2(0, -200);
            }

            if (AttackTimer <= 75)
            {
                NPC.direction = NPC.velocity.X < 0 ? -1 : 1;
                JumpToTarget(1, 75, yStrength: 0.85f);
            }

            NPC.noGravity = AttackTimer > 55 && AttackTimer < bubbleRecoil;

            if (AttackTimer > 55 && AttackTimer < 80)
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * Math.Max(NPC.Distance(moveTarget), 3f), 0.1f) * 0.3f;

            Vector2 staffPos = NPC.Center + new Vector2(8 * NPC.direction, -92).RotatedBy(NPC.rotation);

            if (AttackTimer == 80)
            {
                NPC.FaceTarget();
                bubbleIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, Vector2.Zero, ProjectileType<GlassBubble>(), 50, 2f, Main.myPlayer);
            }

            if (AttackTimer < 300 && AttackTimer > 80)
                Main.projectile[bubbleIndex].Center = staffPos;

            if (AttackTimer == 300)
                moveTarget = Main.projectile[bubbleIndex].Center;

            if (AttackTimer > 300 && AttackTimer < bubbleRecoil - 1)
            {
                NPC.FaceTarget();
                if (AttackTimer < 400)
                {
                    if (AttackTimer > 390)
                        NPC.Center += new Vector2(10, 0).RotatedBy(NPC.AngleTo(moveTarget));
                    else
                        NPC.Center = Vector2.Lerp(NPC.Center, moveTarget - new Vector2(160, 0).RotatedBy(moveTarget.AngleTo(Target.Center)), 0.05f);
                }

                NPC.rotation = Helpers.Helper.BezierEase(Utils.GetLerpValue(360, 395, AttackTimer, true)) * (NPC.AngleTo(moveTarget)- MathHelper.PiOver2);
            }

            if (AttackTimer == bubbleRecoil - 1)
                moveTarget = PickSideFromMe();
 
            if (AttackTimer > bubbleRecoil - 1)
            {
                if (AttackTimer <= bubbleRecoil + 50)
                    SpinJumpToTarget(bubbleRecoil, bubbleRecoil + 50, 10, -1);
                
                NPC.velocity.X *= 0.87f;
                NPC.FaceTarget();
            }

            if (AttackTimer > 600)
                ResetAttack();
        }

        private void KickBubble()
        {
            Projectile bubble = Main.projectile[bubbleIndex];
            if (Main.projectile.IndexInRange(bubbleIndex))
            {
                if (bubble.active && bubble.type == ProjectileType<GlassBubble>())
                {

                }
            }
        }
    }
}
