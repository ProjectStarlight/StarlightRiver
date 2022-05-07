using Microsoft.Xna.Framework;
using StarlightRiver.Core;
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

        private void ResetAttack()
        {
            AttackTimer = 0;
            TryEndFight();
        }

        private void TryEndFight()
        {
            if (NPC.life < 1)
                Phase = (int)PhaseEnum.DeathEffects;

            NPC.TargetClosest();
            if (Target.dead || !Target.active || Target == null)
                Phase = (int)PhaseEnum.DespawnEffects;
        }

        //according to the targets position,
        private Vector2 PickSide(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-510 * x, 40) : arenaPos + new Vector2(510 * x, 40); //picks the outer side.

        private Vector2 PickCloseSide(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-140 * x, 0) : arenaPos + new Vector2(140 * x, 0); //picks the inner side.

        private Vector2 PickSideSelf(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(510 * x, 40) : arenaPos + new Vector2(-510 * x, 40); //picks the outer side.

        private Vector2 PickCloseSideSelf(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(140 * x, 0) : arenaPos + new Vector2(-140 * x, 0); //picks the inner side.

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
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 20;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
            }
        }

        private void JumpToTarget(int timeStart, int timeEnd, float yStrength = 1f)
        {
            AttackType = (int)AttackEnum.Jump;
            float progress = Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true);

            if (AttackTimer <= timeStart)
                moveStart = NPC.position;
            if (AttackTimer == timeStart + 1)
                NPC.velocity.Y = -(10f + ((NPC.Center.Y - moveTarget.Y) / 64f)) * yStrength;

            if (progress <= 0.6f)
            {
                NPC.velocity.X = (moveStart.X - moveTarget.X) * -0.015f;
                moveStart.X += NPC.velocity.X * 0.15f;
            }
            else
                NPC.velocity.Y += progress * progress * 0.7f;

            if (AttackTimer >= timeStart && AttackTimer <= timeEnd)
                NPC.position.X = MathHelper.SmoothStep(moveStart.X, moveTarget.X, progress) - (NPC.width / 2f);
        }

        private void SpinJumpToTarget(int timeStart, int timeEnd, float totalRotations = 5, int direction = 1)
        {
            JumpToTarget(timeStart, timeEnd, 0.9f);
            AttackType = (int)AttackEnum.SpinJump;
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
                NPC.velocity.Y = -Helpers.Helper.SwoopEase(Utils.GetLerpValue(0, 15, AttackTimer, true)) * 2f;

            moveTarget.X = MathHelper.Lerp(moveTarget.X, Target.Center.X, 0.005f);

            if (AttackTimer > 10 && AttackTimer < 210 + (spearCount * betweenSpearTime))
            {
                NPC.noGravity = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * NPC.Distance(moveTarget), 0.1f) * 0.2f;
                NPC.Center += new Vector2((float)Math.Sin(AttackTimer * 0.04f % MathHelper.TwoPi), (float)Math.Cos(AttackTimer * 0.04f % MathHelper.TwoPi)) * 0.2f;
            }
            else
                NPC.velocity.X *= 0.8f;

            if (AttackTimer % betweenSpearTime == 0 && AttackTimer >= spearSpawn && AttackTimer < spearSpawn + (spearCount * betweenSpearTime))
            {
                NPC.FaceTarget();
                float whatSpear = (AttackTimer - spearSpawn) / spearCount;

                Vector2 staffPos = NPC.Center + new Vector2(28 * NPC.direction, -92).RotatedBy(NPC.rotation);
                Vector2 spearTarget = PickCloseSide(-1) + new Vector2(whatSpear * 100 * NPC.direction, 40);
                Vector2 spearVel = new Vector2(5f * NPC.direction, 3f).RotatedBy(whatSpear * 4f);
                float angle = (staffPos).AngleTo(spearTarget - (spearVel * 5f));

                Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, spearVel, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, angle);
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
                moveTarget = PickSideSelf(-1);
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
                moveTarget = PickCloseSideSelf();
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

            if (AttackTimer < 75)
            {
                NPC.direction = NPC.velocity.X < 0 ? -1 : 1;
                JumpToTarget(2, 70);
            }

            NPC.noGravity = AttackTimer > 75 && AttackTimer < bubbleRecoil;

            Vector2 staffPos = NPC.Center + new Vector2(8 * NPC.direction, -92).RotatedBy(NPC.rotation);

            if (AttackTimer == 80)
            {
                bubbleIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, Vector2.Zero, ProjectileType<GlassBubble>(), 50, 2f, Main.myPlayer, 0, NPC.whoAmI);
                NPC.velocity.Y -= 4f;
                NPC.velocity.X = 0;
            }

            if (AttackTimer <= 300 && AttackTimer > 80)
            {
                Main.projectile[bubbleIndex].Center = staffPos + (Main.rand.NextVector2Circular(3, 3) * Utils.GetLerpValue(220, 120, AttackTimer, true));
                NPC.velocity *= 0.93f;
                NPC.velocity.Y -= 0.01f;
                NPC.velocity.X += NPC.DirectionTo(moveTarget).X * 0.01f;
            }

            if (AttackTimer == 300)
                moveTarget = Main.projectile[bubbleIndex].Center;

            if (AttackTimer > 300 && AttackTimer < bubbleRecoil - 1)
            {
                NPC.FaceTarget();
                Vector2 target = Vector2.Lerp(Target.Center, PickSide(-1), 0.7f);
                if (AttackTimer < bubbleRecoil - 35)
                {
                    if (AttackTimer > bubbleRecoil - 40)
                        NPC.Center += new Vector2(10, 0).RotatedBy(NPC.AngleTo(moveTarget));
                    else
                        NPC.Center = Vector2.Lerp(NPC.Center, moveTarget - new Vector2(120, 0).RotatedBy(moveTarget.AngleTo(target)), 0.05f);
                }
                else if (AttackTimer == bubbleRecoil - 30)
                    HitBubble(NPC.AngleTo(PickSide(-1)).ToRotationVector2());

                //NPC.rotation = Helpers.Helper.BezierEase(Utils.GetLerpValue(360, 395, AttackTimer, true)) * (NPC.AngleTo(moveTarget)- MathHelper.PiOver2);
            }

            if (AttackTimer == bubbleRecoil - 1)
                moveTarget = PickSideSelf(-1);
 
            if (AttackTimer > bubbleRecoil - 1)
            {
                if (AttackTimer <= bubbleRecoil + 50)
                    SpinJumpToTarget(bubbleRecoil, bubbleRecoil + 50, 10, -1);
                
                NPC.velocity.X *= 0.87f;
                NPC.FaceTarget();
            }

            if (AttackTimer > 630)
                ResetAttack();
        }

        private void HitBubble(Vector2 direction)
        {
            Projectile bubble = Main.projectile[bubbleIndex];
            float speed = 4f;
            if (Main.projectile.IndexInRange(bubbleIndex))
            {
                if (bubble.active && bubble.type == ProjectileType<GlassBubble>())
                {
                    Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 1f, 0f, NPC.Center);
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;
                    bubble.velocity = direction * speed;
                    bubble.ai[0] = 1;
                }
            }
        }
    }
}
