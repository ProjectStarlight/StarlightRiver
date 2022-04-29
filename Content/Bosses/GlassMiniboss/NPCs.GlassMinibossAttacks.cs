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

        private void Knives()
		{
            if (AttackTimer == 1)
                NPC.TargetClosest();

            if (AttackTimer == 5)
            {
                for (int k = 0; k < 9; k++)
                {
                    Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(),NPC.Center + new Vector2(0, -120), Vector2.Zero, ProjectileType<GlassKnife>(), 15, 1, Main.myPlayer, NPC.target, (k - 7) * 0.05f);
                }
            }

            if (AttackTimer >= 90)
                ResetAttack();
		}

        //old hammer attack
        //private void Hammer()
        //{
        //    if (AttackTimer < 10) 
        //        NPC.velocity *= 0.8f; //decelerate into position

        //    if (AttackTimer == 30) //stop and spawn Projectile
        //    {
        //        NPC.velocity *= 0;
        //        Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), NPC.Center, Vector2.Zero, ProjectileType<GlassHammer>(), 40, 1, Main.myPlayer, NPC.direction, NPC.whoAmI);
        //    }

        //    if (AttackTimer >= 110)
        //        ResetAttack();
        //}

        //old spears attack
        //private void Spears() //summon a wall of spears on one side of the arena, tentative keep?
        //{
        //    if (AttackTimer == 1)
        //    {
        //        NPC.TargetClosest();
        //        moveTarget = PickSide();
        //        moveStart = NPC.Center;
        //    }

        //    if (AttackTimer < 60) //go to the side away from the target
        //        NPC.Center = Vector2.SmoothStep(moveStart, moveTarget, AttackTimer / 60f);

        //    if (AttackTimer == 90) //spawn the Projectiles
        //    {
        //        int exclude = Main.rand.Next(3);
        //        for (int k = 0; k < 6; k++)
        //        {
        //            if ((k / 2) != exclude) //leave an opening!
        //                Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), NPC.Center, Vector2.Zero, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, k * 60, Direction);
        //        }
        //    }

        //    if (AttackTimer >= 210)
        //        ResetAttack();
        //}

        private void Spears()
        {
            AttackType = (int)AttackEnum.Spears;

            int spearCount = 12;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveStart = NPC.Center;
                moveTarget = PickSideClose() + new Vector2(0, 50);
            }

            if (AttackTimer > 1 && AttackTimer < 20)
                NPC.velocity.Y = Helpers.Helper.BezierEase((20 - (AttackTimer - 5)) / 20f) * -0.02f * Math.Max(NPC.Distance(moveTarget), 3f);

            if (AttackTimer > 10)
            {
                NPC.noGravity = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(moveTarget) * Math.Max(NPC.Distance(moveTarget), 3f), 0.05f) * 0.2f * Utils.GetLerpValue(10, 20, AttackTimer, true);
                NPC.Center += new Vector2((float)Math.Sin(AttackTimer * 0.04f % MathHelper.TwoPi), (float)Math.Cos(AttackTimer * 0.04f % MathHelper.TwoPi)) * 0.2f;
                if (AttackTimer < 70)
                    PickSide(); NPC.FaceTarget();
            }

            const int spearStart = 90;
            if (AttackTimer % 5 == 0 && AttackTimer >= spearStart && AttackTimer < spearStart + (spearCount * 5f))
            {
                Vector2 staffPos = NPC.Center + new Vector2(40 * NPC.direction, -70).RotatedBy(NPC.rotation);
                Vector2 spearTarget = PickSide(-1) + new Vector2((-360 + (200f / spearCount * ((AttackTimer - spearStart)))) * NPC.direction, 20);
                Vector2 spearVel = Main.rand.NextVector2CircularEdge(3, 3) + Main.rand.NextVector2Circular(2, 2);
                Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), staffPos, spearVel, ProjectileType<GlassSpear>(), 30, 1, Main.myPlayer, staffPos.AngleTo(spearTarget));
            }

            if (AttackTimer >= 300)
                ResetAttack();
        }

        private void Hammer()
        {
            AttackType = (int)AttackEnum.Hammer;

            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                moveTarget = PickSide();
                moveStart = NPC.Center;
            }

            if (AttackTimer > 1 && AttackTimer < 40)
                NPC.velocity.Y -= 10;

            if (AttackTimer > 1 && AttackTimer < 20)
                NPC.velocity.X = NPC.direction * 5f;
            else
                NPC.velocity.X *= 0.8f;

            if (AttackTimer > 300)
                ResetAttack();

        }
    }
}
