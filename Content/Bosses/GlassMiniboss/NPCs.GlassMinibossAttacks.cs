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
            if (NPC.life <= 1)
                Phase = (int)PhaseEnum.DeathEffects;

            NPC.TargetClosest();
            if (Target.dead || !Target.active || Target == null)
                Phase = (int)PhaseEnum.DespawnEffects;
        }

        //according to the targets position,
        private const int distX = 520;
        private const int distY = 30;
        private const int distShortX = 130;
        private const int distShortY = -10;

        private Vector2 PickSide(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-distX * x, distY) : arenaPos + new Vector2(distX * x, distY); //picks the outer side.

        private Vector2 PickCloseSide(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-distShortX * x, -distShortY) : arenaPos + new Vector2(distShortX * x, -distShortY); //picks the inner side.

        private Vector2 PickSideSelf(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(distX * x, distY) : arenaPos + new Vector2(-distX * x, distY); //picks the outer side.

        private Vector2 PickCloseSideSelf(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(distShortX * x, -distShortY) : arenaPos + new Vector2(-distShortX * x, -distShortY); //picks the inner side.

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

        private int jumpStart;
        private int jumpEnd;

        private void JumpToTarget(int timeStart, int timeEnd, float yStrength = 1f, bool spin = false)
        {
            jumpStart = timeStart;
            jumpEnd = timeEnd;

            float progress = Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true);
            if (!spin)
                AttackType = (int)AttackEnum.Jump;
            else
                AttackType = (int)AttackEnum.SpinJump;

            if (AttackTimer == timeStart + 1)
            {
                moveStart = NPC.Center;
                NPC.velocity.Y = -((10f * Utils.GetLerpValue(-250, 150, moveStart.Distance(moveTarget), true)) - ((float)Math.Abs(moveStart.Y - moveTarget.Y) / 24f)) * yStrength;
            }

            if (progress <= 0.6f)
                moveStart.X += NPC.velocity.X * 0.15f;
            else
                NPC.velocity.Y += progress * progress * 0.7f;

            if (AttackTimer >= timeStart && AttackTimer <= timeEnd)
                NPC.position.X = MathHelper.SmoothStep(moveStart.X, moveTarget.X, progress) - (NPC.width / 2f);//why isnt Center settable
            else
                NPC.velocity.X *= 0.01f;
        }

        private void SpinJumpToTarget(int timeStart, int timeEnd, float totalRotations = 5, int direction = 1)
        {
            JumpToTarget(timeStart, timeEnd, 0.9f, true);
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
                spearCount = 15;
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

            moveTarget.X = MathHelper.Lerp(moveTarget.X, Target.Center.X, 0.002f);

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

                Vector2 staffPos = NPC.Center + new Vector2(32 * NPC.direction, -105).RotatedBy(NPC.rotation);
                Vector2 spearTarget = arenaPos + new Vector2(whatSpear * 130 * NPC.direction, 40);
                Vector2 spearVel = new Vector2(Main.rand.NextFloat(9, 12) * NPC.direction, 3f).RotatedBy(whatSpear * 4f);
                float angle = (staffPos).AngleTo(spearTarget - (spearVel * 2f));

                Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, spearVel, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, angle);
            }

            if (AttackTimer > spearTime)
                ResetAttack();
        }

        private void Hammer()
        {
            AttackType = (int)AttackEnum.Hammer;
            hammerTime = 80;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickSide(-1);
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer <= 75)
            {
                JumpToTarget(2, 75);
                NPC.velocity.X = -Direction * 0.3f;
                NPC.direction = Direction;
            }

            if (!(AttackTimer > 75 && AttackTimer < 100))
                NPC.velocity.X *= 0.7f;

            if (AttackTimer == hammerSpawn)
                hammerIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.whoAmI, hammerTime);
            
            int spikeCount = 5;
            if (Main.masterMode)
                spikeCount = 12;
            else if (Main.expertMode)
                spikeCount = 8;

            int spikeSpawn = hammerSpawn + hammerTime - 65;
            int betweenSpikes = 2;
            float dist = Utils.GetLerpValue(spikeSpawn - 1.5f, spikeSpawn + (spikeCount * betweenSpikes), AttackTimer, true);
            if (AttackTimer >= spikeSpawn && AttackTimer < spikeSpawn + (spikeCount * betweenSpikes) && AttackTimer % betweenSpikes == 0)
            {
                Vector2 spikeX = Vector2.Lerp(PickSideSelf(), new Vector2(PickSideSelf(-1).X + (102 * Direction), NPC.Center.Y), dist);
                Vector2 spikePos = new Vector2(spikeX.X, NPC.Top.Y - 100);//half the spike's height
                Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaisedSpike>(), 40, 1f, Main.myPlayer, -20, dist);
                raise.direction = NPC.direction;
            }

            if (AttackTimer > spikeSpawn + (spikeCount * betweenSpikes) + 120)
                ResetAttack();

        }

        //spikes out from center instead of side
        private void HammerVariant()
        {
            AttackType = (int)AttackEnum.Hammer;
            hammerTime = 110;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickCloseSide();
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer <= 75)
            {
                JumpToTarget(2, 75);
                NPC.velocity.X = -Direction * 0.3f;
                NPC.direction = Direction;
            }

            if (!(AttackTimer > 75 && AttackTimer < 100))
                NPC.velocity.X *= 0.7f;

            if (AttackTimer == hammerSpawn)
                hammerIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.whoAmI, hammerTime);

            int spikeCount = 3;
            if (Main.masterMode)
                spikeCount = 6;
            else if (Main.expertMode)
                spikeCount = 4;

            int spikeSpawn = hammerSpawn + hammerTime - 65;
            int betweenSpikes = 2;
            float dist = Utils.GetLerpValue(spikeSpawn - 1.5f, spikeSpawn + (spikeCount * betweenSpikes), AttackTimer, true);
            if (AttackTimer >= spikeSpawn - betweenSpikes && AttackTimer < spikeSpawn + (spikeCount * betweenSpikes) && AttackTimer % betweenSpikes == 0)
            {
                Vector2 spikeX = Vector2.Lerp(arenaPos, new Vector2(PickSideSelf(-1).X + (102 * Direction), NPC.Center.Y), dist);
                Vector2 spikePos = new Vector2(spikeX.X, NPC.Top.Y - 100);//half the spike's height
                Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaisedSpike>(), 40, 1f, Main.myPlayer, -20, dist);
                raise.direction = NPC.direction;
            }
            if (AttackTimer >= spikeSpawn && AttackTimer < spikeSpawn + (spikeCount * betweenSpikes) && (AttackTimer + 1) % betweenSpikes == 0)
            {
                Vector2 spikeX = Vector2.Lerp(arenaPos, new Vector2(PickSideSelf().X + (102 * -Direction), NPC.Center.Y), dist);
                Vector2 spikePos = new Vector2(spikeX.X, NPC.Top.Y - 100);//half the spike's height
                Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaisedSpike>(), 40, 1f, Main.myPlayer, -20, dist);
                raise.direction = -NPC.direction;
            }

            if (AttackTimer > spikeSpawn + (spikeCount * betweenSpikes) + 120)
                ResetAttack();
        }

        private void BigBrightBubble()
        {
            AttackType = (int)AttackEnum.BigBrightBubble;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveStart = NPC.Center;
                moveTarget = PickCloseSide();
            }

            if (AttackTimer <= 70)
            {
                NPC.FaceTarget();
                JumpToTarget(1, 70);
            }

            NPC.noGravity = AttackTimer > 75 && AttackTimer < bubbleRecoil;

            Vector2 staffPos = NPC.Center + new Vector2(5 * NPC.direction, -100).RotatedBy(NPC.rotation);

            if (AttackTimer == 80)
            {
                NPC.direction = Direction;
                bubbleIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, Vector2.Zero, ProjectileType<GlassBubble>(), 20, 2f, Main.myPlayer, NPC.whoAmI);
                NPC.velocity.Y -= 4f;
            }

            if (AttackTimer <= 300 && AttackTimer > 80)
            {
                Main.projectile[bubbleIndex].Center = staffPos + (Main.rand.NextVector2Circular(3, 3) * Utils.GetLerpValue(220, 120, AttackTimer, true));
                NPC.velocity *= 0.87f;
                NPC.velocity.Y -= 0.01f;
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += (int)(AttackTimer / 200f);
            }

            if (AttackTimer == 300)
                moveTarget = Main.projectile[bubbleIndex].Center;

            if (AttackTimer > 300 && AttackTimer < bubbleRecoil - 1)
            {
                Vector2 target = Vector2.Lerp(Target.Center, PickSideSelf(-1) - new Vector2(0, 70), 0.8f);
                if (AttackTimer < bubbleRecoil - 20)
                {
                    if (AttackTimer > bubbleRecoil - 30)
                        NPC.Center += new Vector2(10, 0).RotatedBy(NPC.AngleTo(moveTarget));
                    else
                        NPC.Center = Vector2.Lerp(NPC.Center, moveTarget - new Vector2(120, 0).RotatedBy(moveTarget.AngleTo(target)), 0.05f);
                }
                else if (AttackTimer == bubbleRecoil - 20)
                    HitBubble(NPC.AngleTo(target).ToRotationVector2());

                NPC.direction = Direction;

                NPC.rotation = (float)Math.Pow(Utils.GetLerpValue(390, bubbleRecoil - 20, AttackTimer, true), 2) * MathHelper.TwoPi * 4f * NPC.direction;
            }

            if (AttackTimer == bubbleRecoil - 1)
            {
                moveTarget = PickSideSelf();
                moveStart = NPC.Center;
            }
 
            if (AttackTimer > bubbleRecoil - 1)
            {
                if (AttackTimer <= bubbleRecoil + 50)
                    SpinJumpToTarget(bubbleRecoil, bubbleRecoil + 50, 3, -1);
                else
                {
                    NPC.velocity.X *= 0.87f;
                    NPC.FaceTarget();
                }
            }

            if (AttackTimer > 630)
                ResetAttack();
        }

        private void HitBubble(Vector2 direction)
        {
            Projectile bubble = Main.projectile[bubbleIndex];
            float speed = 4.77f;
            if (Main.projectile.IndexInRange(bubbleIndex))
            {
                if (bubble.active && bubble.type == ProjectileType<GlassBubble>())
                {
                    Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 1f, 0f, NPC.Center);
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;
                    bubble.velocity = direction * speed;
                    bubble.ai[1] = 1;
                }
            }
        }
    }
}
