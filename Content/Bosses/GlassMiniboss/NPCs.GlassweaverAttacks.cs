using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    public partial class Glassweaver : ModNPC
    {
        Vector2 moveStart;
        Vector2 moveTarget;
        Player Target => Main.player[NPC.target];

        private int javelinTime;
        private int hammerTime;
        private int[] slashTime = new int[] { 70, 105, 120 };

        public int hammerIndex;
        public int bubbleIndex;
        public int whirlIndex;
        public int spearIndex;

        private int jumpStart;
        private int jumpEnd;

        private const int javelinSpawn = 30;
        private const int hammerSpawn = 90;
        private const int bubbleRecoil = 300;
        private const int spotDistX = 520;
        private const int spotDistY = 30;
        private const int spotDistShortX = 130;
        private const int spotDistShortY = -10;

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
        private Vector2 PickSpot(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-spotDistX * x, spotDistY) : arenaPos + new Vector2(spotDistX * x, spotDistY); //picks the outer side.

        private Vector2 PickCloseSpot(int x = 1) => Target.Center.X > arenaPos.X ? arenaPos + new Vector2(-spotDistShortX * x, -spotDistShortY) : arenaPos + new Vector2(spotDistShortX * x, -spotDistShortY); //picks the inner side.

        private Vector2 PickSpotSelf(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(spotDistX * x, spotDistY) : arenaPos + new Vector2(-spotDistX * x, spotDistY); //picks the outer side.

        private Vector2 PickCloseSpotSelf(int x = 1) => NPC.Center.X > arenaPos.X ? arenaPos + new Vector2(spotDistShortX * x, -spotDistShortY) : arenaPos + new Vector2(-spotDistShortX * x, -spotDistShortY); //picks the inner side.

        private Vector2 PickNearestSpot(Vector2 target)
        {
            if (target.Distance(PickSpot(-1)) < target.Distance(PickCloseSpot(-1)))
                return PickSpot(-1);
            return PickCloseSpot(-1);
        }

        private int Direction => NPC.Center.X > arenaPos.X ? -1 : 1;

        private int DirectionAway => Direction * -1;

        private void SpawnAnimation()
        {
            if (AttackTimer < 60)
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

            if (AttackTimer > 60 && AttackTimer < 90)
            {
                NPC.Center = Vector2.Lerp(moveStart, arenaPos + new Vector2(0, -60), (AttackTimer - 60) / 30f);
            }

            if (AttackTimer == 90)
            {
                Core.Systems.CameraSystem.Shake += 20;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
            }
        }

        private void JumpToTarget(int timeStart, int timeEnd, float yStrength = 1f, bool spin = false)
        {
            jumpStart = timeStart;
            jumpEnd = timeEnd;

            if (AttackTimer < timeStart + 5 && Math.Abs(moveStart.X - moveTarget.X) < 8f)
            {
                AttackTimer = timeEnd + 1;
                return;
            }

            float progress = Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true);
            if (!spin)
                AttackType = (int)AttackEnum.Jump;
            else
                AttackType = (int)AttackEnum.SpinJump;

            if (AttackTimer <= timeStart)
            {
                moveStart = NPC.Center;
                NPC.velocity.Y = -MathHelper.Lerp(7f, 8f, moveStart.Distance(moveTarget) * 0.003f) * yStrength;
            }

            if (AttackTimer == timeStart + 3 && !spin && !disableJumpSound)
                Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundJump", 1f, 0.7f, NPC.Center);

            if (progress <= 0.6f)
                moveStart.X += NPC.velocity.X * 0.15f;
            else
                NPC.velocity.Y += (float)Math.Pow(progress * 0.7f, 2);

            if (AttackTimer >= timeStart && AttackTimer <= timeEnd)
                NPC.position.X = MathHelper.Lerp(MathHelper.SmoothStep(moveStart.X, moveTarget.X, MathHelper.Min(progress * 1.1f, 1f)), moveTarget.X, progress) - (NPC.width / 2f);
            else
                NPC.velocity.X *= 0.01f;
        }

        private void SpinJumpToTarget(int timeStart, int timeEnd, float totalRotations = 5, int direction = 1)
        {
            JumpToTarget(timeStart, timeEnd, 0.7f, true);
            float progress = Helpers.Helper.BezierEase(Utils.GetLerpValue(timeStart, timeEnd, AttackTimer, true));
            NPC.rotation = MathHelper.WrapAngle(progress * MathHelper.TwoPi * totalRotations) * NPC.direction * direction;
        }

        private void TripleSlash()
        {
            AttackType = (int)AttackEnum.TripleSlash;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickNearestSpot(Target.Center) - new Vector2(0, 100);
                moveStart = NPC.Center;
            }

            if (AttackTimer < 50)
            {
                NPC.FaceTarget();
                JumpToTarget(2, 50);
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, NPC.DirectionTo(moveTarget).Y * NPC.Distance(moveTarget) * 0.1f, 0.1f);
            }
            
            if (AttackTimer < 65 && AttackTimer > 30)
                NPC.velocity.Y *= 0.3f;

            NPC.noGravity = AttackTimer > 40 && AttackTimer < slashTime[2];

            if (AttackTimer == 48)
            {
                NPC.velocity.X = -NPC.direction * 4f;
                NPC.FaceTarget();
                for (int s = 0; s < 3; s++)
                {
                    int slash = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassSword>(), 10, 0.2f, Main.myPlayer, AttackTimer - 2, NPC.whoAmI);
                    Main.projectile[slash].localAI[0] = s;
                }
            }

            if (AttackTimer < slashTime[2] + 60 && AttackTimer > 40)
            {
                NPC.velocity.X *= 0.92f;
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, (Target.Top - NPC.Center).Y * 0.001f, 0.05f);
            }

            if (AttackTimer == slashTime[0] || AttackTimer == slashTime[1] || AttackTimer == slashTime[2] - 1)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassSlash", 1f, 0.1f, NPC.Center);
                if (AttackTimer == slashTime[2] - 1)
                {
                    NPC.TargetClosest();
                    NPC.FaceTarget();
                }
                NPC.velocity.X += NPC.direction * MathHelper.Lerp(7f, 30f, (float)Math.Pow((AttackTimer - slashTime[0] - 1) / 80f, 2f));
            }

            if (AttackTimer > slashTime[2] + 50)
                ResetAttack();
        }

        private void MagmaSpear()
        {
            AttackType = (int)AttackEnum.MagmaSpear;
            int lobCount = 5;

            if (Main.masterMode)
                lobCount = 15;
            else if (Main.expertMode)
                lobCount = 8;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveStart = NPC.Center;
                moveTarget = Vector2.Lerp(PickSpot(), PickCloseSpot(), 0.77f) - new Vector2(0, 100);
                NPC.velocity.Y -= 9f;
                
                spearIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 10, 0.2f, Main.myPlayer, 0, NPC.whoAmI);
                Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundJump", 1f, 0.7f, NPC.Center);
            }

            if (AttackTimer <= 65)
            {
                NPC.FaceTarget();
                float jumpProgress = Utils.GetLerpValue(5, 65, AttackTimer, true);
                NPC.position.X = MathHelper.Lerp(MathHelper.SmoothStep(moveStart.X, moveTarget.X, MathHelper.Min(jumpProgress * 1.1f, 1f)), moveTarget.X, jumpProgress) - (NPC.width / 2f);
                NPC.velocity.Y *= 0.94f;
                NPC.noGravity = true;
            }
            else if (AttackTimer < 85 && !(NPC.collideY || NPC.velocity.Y < 0))
            {
                moveTarget = arenaPos;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * 20, 0.4f);
                NPC.velocity.Y *= 1.5f;
            }
            else
                NPC.velocity.X *= 0.5f;

            if (AttackTimer > 65 && AttackTimer < 90 && NPC.collideY && NPC.velocity.Y > 0)
            {
                AttackTimer = 80;
                Main.projectile[spearIndex].ai[0] = 80;

                Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, NPC.Center);

                Vector2 lobPos = NPC.Bottom + new Vector2(70 * NPC.direction, -2);
                for (int i = 0; i < lobCount; i++)
                {
                    float lobVel = MathHelper.ToRadians(MathHelper.Lerp(17, 76, (float)i / lobCount)) * NPC.direction;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), lobPos, Vector2.Zero, ProjectileType<LavaLob>(), 10, 0.2f, Main.myPlayer, -44 - i, lobVel);
                }
                for (int j = 0; j < 50; j++)
                    Dust.NewDustPerfect(lobPos + Main.rand.NextVector2Circular(20, 1), DustType<Dusts.GlassGravity>(), -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(1f, 6f));
            }

            if (AttackTimer > 220)
                ResetAttack();
        }

        private void Whirlwind()
        {
            AttackType = (int)AttackEnum.Whirlwind;

            //if (AttackTimer == 1)
            //{
            //    NPC.TargetClosest();
            //    moveTarget = PickNearestSpot(Target.Center) - new Vector2(0, 100);
            //}

            //if (AttackTimer < 50)
            //{
            //    NPC.FaceTarget();
            //    JumpToTarget(2, 50, 0.8f);
            //}

            //if (AttackTimer == 80)
            //    whirlIndex = Projectile.NewProjectile(Entity.InheritSource(NPC), NPC.Center, Vector2.Zero, ProjectileType<Whirlwind>(), 12, 0.5f, Main.myPlayer, 0, NPC.whoAmI);

            //if (AttackTimer > 10 && AttackTimer < 80)
            //    NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, NPC.DirectionTo(moveTarget).Y * 5f, 0.1f);
            //else if (AttackTimer > 80)
            //{
            //    NPC.velocity += NPC.DirectionTo(moveTarget) * 0.01f;
            //    if (AttackTimer < 100)
            //        moveTarget = Target.Center;
            //}

            //if (AttackTimer > 120)
            //    NPC.velocity *= 0.85f;
            //else if (AttackTimer > 100)
            //    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * 10, 0.5f);


            //NPC.noGravity = AttackTimer > 30 && AttackTimer < 160;
            //if (NPC.noGravity)
            //    NPC.velocity.Y *= 0.8f;

            ResetAttack();
        }

        private void JavelinRain()
        {
            AttackType = (int)AttackEnum.JavelinRain;

            int spearCount = 10;
            int betweenSpearTime = 5;

            if (Main.masterMode)
            {
                spearCount = 14;
                betweenSpearTime = 4;
            }

            javelinTime = 50 + javelinSpawn + (spearCount * betweenSpearTime);

            NPC.TargetClosest();
            NPC.FaceTarget();

            if (AttackTimer == 1)
            {
                moveStart = NPC.Center;
                moveTarget = PickCloseSpot() - new Vector2(0, 100);
            }

            if (AttackTimer > 1 && AttackTimer < 25)
                NPC.velocity.Y = -(Utils.GetLerpValue(25, 10, AttackTimer, true)) * 5f;

            moveTarget.X = MathHelper.Lerp(moveTarget.X, Target.Center.X, 0.002f);

            if (AttackTimer > 10 && AttackTimer < javelinTime - javelinSpawn)
            {
                NPC.noGravity = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * NPC.Distance(moveTarget), 0.01f) * 0.5f;
                NPC.Center += new Vector2((float)Math.Sin(AttackTimer * 0.04f % MathHelper.TwoPi), (float)Math.Cos(AttackTimer * 0.04f % MathHelper.TwoPi)) * 0.2f;
            }
            else
                NPC.velocity.X *= 0.8f;

            if (AttackTimer % betweenSpearTime == 0 && AttackTimer >= javelinSpawn && AttackTimer < javelinSpawn + (spearCount * betweenSpearTime))
            {
                NPC.FaceTarget();
                float whatSpear = (AttackTimer - javelinSpawn) / spearCount;

                Vector2 staffPos = NPC.Center + new Vector2(32 * NPC.direction, -105).RotatedBy(NPC.rotation);
                Vector2 spearTarget = Target.Center;//arenaPos + new Vector2(whatSpear * 130 * NPC.direction, 40);
                Vector2 spearVel = new Vector2(Main.rand.NextFloat(9, 12) * NPC.direction, 3f).RotatedBy(whatSpear * 4f);
                float angle = (staffPos).AngleTo(spearTarget - (spearVel * 2f));
                Projectile.NewProjectile(NPC.GetSource_FromAI(), staffPos, spearVel, ProjectileType<GlassJavelin>(), 12, 1, Main.myPlayer, angle);
            }

            if (AttackTimer > javelinTime)
                ResetAttack();
        }

        private void GlassRaise()
        {
            AttackType = (int)AttackEnum.GlassRaise;
            hammerTime = 80;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickSpot();
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
            int betweenSpikes = 5;
            float dist = Utils.GetLerpValue(spikeSpawn - 1.5f, spikeSpawn + (spikeCount * betweenSpikes), AttackTimer, true);
            if (AttackTimer >= spikeSpawn && AttackTimer < spikeSpawn + (spikeCount * betweenSpikes) && AttackTimer % betweenSpikes == 0)
            {
                float spikeX = MathHelper.Lerp(PickSpotSelf().X, PickSpotSelf(-1).X + (102 * Direction), dist);
                Vector2 spikePos = new Vector2(spikeX, arenaPos.Y - 100);
                Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 20, 1f, Main.myPlayer, -20, dist);
                raise.direction = NPC.direction;
            }

            if (AttackTimer > spikeSpawn + (spikeCount * betweenSpikes) + 120)
                ResetAttack();

        }

        //spikes out from center instead of side
        private void GlassRaiseAlt()
        {
            AttackType = (int)AttackEnum.GlassRaise;
            hammerTime = 110;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickCloseSpot();
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer <= 75)
            {
                JumpToTarget(3, 75, 1.2f);
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
            int betweenSpikes = 5;
            float dist = Utils.GetLerpValue(spikeSpawn - 1.5f, spikeSpawn + (spikeCount * betweenSpikes), AttackTimer, true);
            if (AttackTimer >= spikeSpawn - 1 && AttackTimer < spikeSpawn + (spikeCount * betweenSpikes) && AttackTimer % betweenSpikes == 0)
            {
                float spikeX = MathHelper.Lerp(arenaPos.X, PickSpotSelf(-1).X + (102 * Direction), dist);
                Vector2 spikePos = new Vector2(spikeX, arenaPos.Y - 120);
                Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 40, 1f, Main.myPlayer, -20, dist);
                raise.direction = NPC.direction;
            }
            if (AttackTimer >= spikeSpawn && AttackTimer < spikeSpawn + (spikeCount * betweenSpikes) && (AttackTimer - 1) % betweenSpikes == 0)
            {
                float spikeX = MathHelper.Lerp(arenaPos.X, PickSpotSelf().X + (102 * -Direction), dist);
                Vector2 spikePos = new Vector2(spikeX, arenaPos.Y - 120);
                Projectile raise = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), spikePos, Vector2.Zero, ProjectileType<GlassRaiseSpike>(), 40, 1f, Main.myPlayer, -20, dist);
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
                moveTarget = PickCloseSpot();
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
                NPC.velocity.Y -= 1.5f;
            }

            if (AttackTimer <= 240 && AttackTimer > 80)
            {
                Main.projectile[bubbleIndex].Center = staffPos + (Main.rand.NextVector2Circular(3, 3) * Utils.GetLerpValue(220, 120, AttackTimer, true));
                NPC.velocity *= 0.87f;
                NPC.velocity.Y -= 0.01f;
                Core.Systems.CameraSystem.Shake += (int)(AttackTimer / 180f);
            }

            if (AttackTimer == 240)
                moveTarget = Main.projectile[bubbleIndex].Center;

            if (AttackTimer > 240 && AttackTimer < bubbleRecoil - 1)
            {
                Vector2 target = Vector2.Lerp(Target.Center, PickSpotSelf(-1) - new Vector2(0, 150), 0.8f);
                if (AttackTimer < bubbleRecoil - 20)
                {
                    if (AttackTimer > bubbleRecoil - 30)
                        NPC.Top = Vector2.SmoothStep(NPC.Top, moveTarget - new Vector2(150 * Utils.GetLerpValue(bubbleRecoil - 23, bubbleRecoil - 30, AttackTimer, true), 0).RotatedBy(moveTarget.AngleTo(target)), 0.3f);
                    else
                        NPC.Top = Vector2.Lerp(NPC.Top, moveTarget - new Vector2(120, 0).RotatedBy(moveTarget.AngleTo(target)), 0.05f);
                }
                else if (AttackTimer == bubbleRecoil - 20)
                    HitBubble(NPC.DirectionTo(target));

                NPC.direction = Direction;

                NPC.rotation = (float)Math.Pow(Utils.GetLerpValue(390, bubbleRecoil - 20, AttackTimer, true), 2) * MathHelper.TwoPi * 4f * NPC.direction;
            }

            if (AttackTimer == bubbleRecoil - 1)
                moveTarget = PickSpotSelf();

            if (AttackTimer > bubbleRecoil - 1)
            {
                if (AttackTimer <= bubbleRecoil + 50)
                {
                    SpinJumpToTarget(bubbleRecoil, bubbleRecoil + 50, 3, -1);
                    NPC.velocity.Y += 0.06f;
                }
                else
                {
                    NPC.velocity.X *= 0.87f;
                    NPC.FaceTarget();
                }
            }

            if (AttackTimer > bubbleRecoil + 80)
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
                    Core.Systems.CameraSystem.Shake += 10;
                    bubble.velocity = direction * speed;
                    bubble.ai[1] = 1;
                    for (int i = 0; i < 30; i++)
                        Dust.NewDustPerfect(bubble.Center, DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(6, 3).RotatedBy(NPC.AngleTo(bubble.Center)), 0, GlassColor);
                }
            }
        }
    }
}
