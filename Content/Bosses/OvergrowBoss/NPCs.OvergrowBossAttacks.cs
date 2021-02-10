using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.NPCs.Boss.OvergrowBoss;
using StarlightRiver.Helpers;
using StarlightRiver.NPCs.Boss.OvergrowBoss.OvergrowBossProjectile;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
    public partial class OvergrowBoss : ModNPC
    {
        private void RandomizeTarget()
        {
            List<int> players = new List<int>();
            foreach (Player player in Main.player.Where(p => Vector2.Distance(npc.Center, p.Center) < 2000)) players.Add(player.whoAmI);
            if (players.Count == 0) return;
            npc.target = players[Main.rand.Next(players.Count)];
        }

        public void ResetAttack()
        {
            flail.npc.velocity *= 0;
            AttackTimer = 0;
        }

        #region phase 1
        private void Phase1Spin()
        {
            if (AttackTimer <= 60)
            {
                npc.Center = new Vector2(npc.Center.X, Vector2.Lerp(npc.Center, spawnPoint, AttackTimer / 60f).Y);
                flail.npc.Center = Vector2.SmoothStep(flail.npc.Center, npc.Center, AttackTimer / 60f);
            }

            if (AttackTimer == 61)
            {
                npc.TargetClosest();
                targetPoint = Main.player[npc.target].Center;
            }

            float size = Vector2.Distance(targetPoint, npc.Center);
            if (size > 400) size = 400;

            if (AttackTimer > 61)
            {
                //following in X direction only
                Player player = Main.player[npc.target];

                if (player.Center.X > npc.Center.X) npc.velocity.X += 0.2f;
                else npc.velocity.X -= 0.2f;

                if (npc.velocity.LengthSquared() > 16) npc.velocity = Vector2.Normalize(npc.velocity) * 4;

                //movement of the flail
                float progress = 0;
                if (AttackTimer < 180) progress = (AttackTimer - 60) / 120f;
                if (AttackTimer >= 180 && AttackTimer < 400) progress = 1;
                if (AttackTimer >= 400) progress = (60 - (AttackTimer - 400)) / 60f;

                float rot = 3.14f + (AttackTimer - 60) / 400f * 6.28f * 6;
                Vector2 target = Vector2.SmoothStep(npc.Center, npc.Center + Vector2.UnitY.RotatedBy(rot) * size, progress);
                flail.npc.Center = target;

                //dust
                for (int k = 0; k < 3; k++) Dust.NewDust(flail.npc.position, flail.npc.width, flail.npc.height, DustType<Content.Dusts.GoldWithMovement>());
                for (int k = 0; k < 8; k++) Dust.NewDustPerfect(Vector2.Lerp(flail.npc.Center, flail.npc.oldPosition + flail.npc.Size / 2, k / 8f), DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * 0.5f);
            }

            if (AttackTimer > 400) //deceleration
            {
                float length = 4 - (AttackTimer - 400) / 90f * 4;
                if (npc.velocity.LengthSquared() > length * length) npc.velocity = Vector2.Normalize(npc.velocity) * length;
            }

            if (AttackTimer >= 490) ResetAttack();
        }

        private void Phase1Bolts()
        {
            Vector2 handpos = npc.Center; //used as a basepoint for this attack to match the animation

            if (AttackTimer <= 30)
            {
                float rot = Main.rand.NextFloat(6.28f); //random rotation for the dust
                Dust.NewDustPerfect(handpos + Vector2.One.RotatedBy(rot) * 50, DustType<Content.Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 2); //"suck in" charging effect
            }

            if (AttackTimer == 30)
            {
                RandomizeTarget(); //pick a random target
                if (Main.player[npc.target] == null) //safety check
                {
                    ResetAttack();
                    return;
                }
            }

            if (AttackTimer == 60) targetPoint = Main.player[npc.target].Center;

            if (AttackTimer >= 60 && AttackTimer <= 120 && AttackTimer % 30 == 0) //3 rounds of projectiles
            {
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ProjectileLaunch1"), npc.Center);
                for (float k = -0.6f; k <= 0.6f; k += 0.3f) //5 projectiles in even spread
                {
                    Vector2 trajectory = Vector2.Normalize(targetPoint - handpos).RotatedBy(k + (AttackTimer == 90 ? 0.15f : 0)) * 1.6f; //towards the target, alternates on the second round
                    Projectile.NewProjectile(handpos, trajectory, ProjectileType<Phase1Bolt>(), 20, 0.2f); //TODO: Relocate or delete that projectile ,or whatever this whole fight is prolly gonna get redone later anyways
                }
            }

            if (AttackTimer == 200) ResetAttack();
        }

        private void Phase1Toss()
        {
            if (AttackTimer == 1)
            {
                npc.TargetClosest();
                npc.velocity.X = Main.player[npc.target].Center.X > npc.Center.X ? 16 : -16;
            }

            if (AttackTimer <= 60)
            {
                if (npc.velocity.X > 0 && npc.Center.X > Main.player[npc.target].Center.X) npc.velocity.X *= 0.9f;
                if (npc.velocity.X < 0 && npc.Center.X < Main.player[npc.target].Center.X) npc.velocity.X *= 0.9f;

                flail.npc.Center = Vector2.Lerp(flail.npc.Center, npc.Center, AttackTimer / 60);
            }

            if (AttackTimer == 60)
            {
                npc.velocity *= 0;

                if (Main.player[npc.target] == null) { ResetAttack(); return; } //safety check

                targetPoint = Main.player[npc.target].Center + Main.player[npc.target].velocity * 30; //sets the target to the closest player

                if (Vector2.Distance(Main.player[npc.target].Center, targetPoint) > 300)
                    targetPoint = Main.player[npc.target].Center + Vector2.Normalize(Main.player[npc.target].Center + targetPoint) * 300; //clamp to 300 pixels away
            }

            Vector2 trajectory = -Vector2.Normalize(flail.npc.Center - targetPoint); //boss' toss direction

            if (AttackTimer > 60 && AttackTimer < 120)
            {
                float time = AttackTimer - 60;
                float rot = 0.418667f * time - 0.00348889f * (float)Math.Pow(time, 2); //quadratic regression of {0, 0}, {60, 12.56}, {120, 0} over the range [0, 60]

                flail.npc.Center = npc.Center + -Vector2.UnitY.RotatedBy(rot) * (AttackTimer - 60) * 1.8f; //spinup animation
            }

            if (AttackTimer >= 120 && AttackTimer < 130) flail.npc.velocity += trajectory * 2; //accelerate the flail towards it's intended path

            if ((flail.npc.velocity.Y == 0 || flail.npc.velocity.X == 0 || Main.tile[(int)flail.npc.Center.X / 16, (int)flail.npc.Center.Y / 16 + 1].collisionType == 1) && !(flail.npc.velocity.Y == 0 && flail.npc.velocity.X == 0)) //hit the ground
            {
                //updates
                flail.npc.velocity *= 0;
                AttackTimer = 180;

                //adds
                for (int k = 0; k < 3; k++)
                    NPC.NewNPC((int)flail.npc.Center.X + Main.rand.Next(-100, 100), (int)flail.npc.Center.Y, NPCType<SkeletonMinion>());

                //visuals
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(flail.npc.position, flail.npc.width, flail.npc.height, DustType<Dusts.Stone>(), Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                    Dust.NewDustPerfect(flail.npc.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, default, 1);
                }

                //audio
                Main.PlaySound(SoundID.Item70, flail.npc.Center);
                Main.PlaySound(SoundID.NPCHit42, flail.npc.Center);

                //screenshake
                int distance = (int)Vector2.Distance(Main.LocalPlayer.Center, flail.npc.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += distance < 500 ? (500 - distance) / 15 : 20;
            }

            if (AttackTimer == 200) ResetAttack();
        }

        private void DrawTossTell(SpriteBatch sb)
        {
            float glow = AttackTimer > 90 ? 1 - (AttackTimer - 90) / 30f : (AttackTimer - 60) / 30f;
            Color color = new Color(255, 70, 70) * glow;
            Texture2D tex = GetTexture("StarlightRiver/Assets/Gores/TellBeam");
            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            for (float k = 0; 1 == 1; k++)
            {
                Vector2 start = npc.Center;
                Vector2 point = Vector2.Lerp(start, start + Vector2.Normalize(targetPoint - npc.Center) * tex.Frame().Width, k);
                sb.Draw(tex, point - Main.screenPosition, tex.Frame(), color, (targetPoint - npc.Center).ToRotation(), tex.Frame().Size() / 2, 1, 0, 0);

                if (!WorldGen.InWorld((int)point.X / 16, (int)point.Y / 16)) break;
                Tile tile = Framing.GetTileSafely(point / 16);
                if (tile.active()) break;
            }
            sb.End();
            sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }

        private void Phase1Trap()
        {
            if (AttackTimer == 1)
            {
                RandomizeTarget();
                targetPoint = Main.player[npc.target].Center + new Vector2(0, -50);
            }

            if (AttackTimer == 90)
            {
                foreach (Player player in Main.player.Where(p => p.active && Helper.CheckCircularCollision(targetPoint, 100, p.Hitbox))) //circular collision
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was strangled..."), 50, 0); //hurt em

                //dusts
                for (float k = 0; k < 6.28f; k += 0.1f)
                {
                    Dust.NewDustPerfect(targetPoint + Vector2.One.RotatedBy(k) * 90, DustType<Dusts.Leaf>(), null, 0, default, 1.5f);
                    Dust.NewDustPerfect(targetPoint + Vector2.One.RotatedBy(k) * Main.rand.NextFloat(95, 105), DustType<Content.Dusts.GoldWithMovement>(), null, 0, default, 0.6f);
                    if (Main.rand.Next(4) == 0) Dust.NewDustPerfect(targetPoint + Vector2.One.RotatedBy(k) * Main.rand.Next(100), DustType<Dusts.Leaf>());
                }
            }

            if (AttackTimer >= 180) ResetAttack();
        }

        private void DrawTrapTell(SpriteBatch sb)
        {
            float glow = AttackTimer > 45 ? 1 - (AttackTimer - 45) / 45f : AttackTimer / 45f;
            Color color = new Color(255, 40, 40) * glow;
            Texture2D tex = GetTexture("StarlightRiver/Assets/Gores/TellCircle");

            if (AttackTimer <= 90) sb.Draw(tex, targetPoint - Main.screenPosition, tex.Frame(), color, 0, tex.Frame().Size() / 2, 2, 0, 0);
            else if (AttackTimer <= 100) sb.Draw(tex, targetPoint - Main.screenPosition, tex.Frame(), new Color(255, 200, 30) * (1 - (AttackTimer - 90) / 10f), 0, tex.Frame().Size() / 2, 2, 0, 0);
        }
        #endregion

        #region phase 2
        private void Phase2Crush()
        {

        }

        private void Phase2WispWall()
        {

        }

        private void Phase2Flail()
        {

        }
        #endregion

        #region phaseless
        private void RapidToss()
        {
            //following in X direction only
            Player player = Main.player[npc.target];

            if (player.Center.X > npc.Center.X) npc.velocity.X += 0.3f;
            else npc.velocity.X -= 0.3f;

            if (npc.velocity.LengthSquared() > 25) npc.velocity = Vector2.Normalize(npc.velocity) * 5;
            npc.velocity.Y = (float)Math.Sin(GlobalTimer / 100f * 6.283f) * 2;

            //attack
            if (AttackTimer <= 15)
                flail.npc.Center = Vector2.Lerp(flail.npc.Center, npc.Center, AttackTimer / 15);
            if (AttackTimer == 15)
            {
                npc.TargetClosest();
                targetPoint = Main.player[npc.target].Center + Main.player[npc.target].velocity * 10; //sets the target to the closest player
                if (Vector2.Distance(Main.player[npc.target].Center, targetPoint) > 300) targetPoint = Main.player[npc.target].Center + Vector2.Normalize(Main.player[npc.target].Center + targetPoint) * 300; //clamp to 3d00 pixels away
            }

            Vector2 trajectory = -Vector2.Normalize(flail.npc.Center - targetPoint); //boss' toss direction

            if (AttackTimer > 15 && AttackTimer < 45)
            {
                float time = 2 * (AttackTimer - 15);
                float rot = 0.418667f * time - 0.00348889f * (float)Math.Pow(time, 2); //quadratic regression of {0, 0}, {60, 12.56}, {120, 0} over the range [0, 60]

                flail.npc.Center = npc.Center + -Vector2.UnitY.RotatedBy(rot) * (AttackTimer - 15) * 2.8f; //spinup animation
            }

            if (AttackTimer >= 45 && AttackTimer < 55) flail.npc.velocity += trajectory * 2.4f;

            if ((flail.npc.velocity.Y == 0 || flail.npc.velocity.X == 0 || Main.tile[(int)flail.npc.Center.X / 16, (int)flail.npc.Center.Y / 16 + 1].collisionType == 1) && !(flail.npc.velocity.Y == 0 && flail.npc.velocity.X == 0)) //hit the ground
            {
                //updates
                flail.npc.velocity *= 0;
                AttackTimer = 85;

                //visuals
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(flail.npc.position, flail.npc.width, flail.npc.height, DustType<Dusts.Stone>(), Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                    Dust.NewDustPerfect(flail.npc.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, default, 1);
                }

                //audio
                Main.PlaySound(SoundID.Item70, flail.npc.Center);
                Main.PlaySound(SoundID.NPCHit42, flail.npc.Center);

                //screenshake
                int distance = (int)Vector2.Distance(Main.LocalPlayer.Center, flail.npc.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += distance < 100 ? distance / 20 : 5;
            }

            if (AttackTimer >= 95) ResetAttack();
        }

        private void DrawRapidTossTell(SpriteBatch sb)
        {
            float glow = AttackTimer < 15 ? AttackTimer / 15f : 1 - (AttackTimer - 15) / 15f;
            Color color = new Color(255, 70, 70) * glow;
            Texture2D tex = GetTexture(AssetDirectory.MiscTextures + "TellBeam");

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            for (float k = 0; 1 == 1; k++)
            {
                Vector2 start = npc.Center;
                Vector2 point = Vector2.Lerp(start, start + Vector2.Normalize(targetPoint - npc.Center) * tex.Frame().Width, k);
                sb.Draw(tex, point - Main.screenPosition, tex.Frame(), color, (targetPoint - npc.Center).ToRotation(), tex.Frame().Size() / 2, 1, 0, 0);

                if (!WorldGen.InWorld((int)point.X / 16, (int)point.Y / 16)) break;
                Tile tile = Framing.GetTileSafely(point / 16);
                if (tile.active()) break;
            }
            sb.End();
            sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
        #endregion
    }
}