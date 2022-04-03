using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.NPCs.Boss.OvergrowBoss;
using StarlightRiver.NPCs.Boss.OvergrowBoss.OvergrowBossProjectile;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
	public partial class OvergrowBoss : ModNPC
    {
        private void RandomizeTarget()
        {
            List<int> Players = new List<int>();
            foreach (Player Player in Main.player.Where(p => Vector2.Distance(NPC.Center, p.Center) < 2000)) Players.Add(Player.whoAmI);
            if (Players.Count == 0) return;
            NPC.target = Players[Main.rand.Next(Players.Count)];
        }

        public void ResetAttack()
        {
            flail.NPC.velocity *= 0;
            AttackTimer = 0;
        }

        #region phase 1
        private void Phase1Spin()
        {
            if (AttackTimer <= 60)
            {
                NPC.Center = new Vector2(NPC.Center.X, Vector2.Lerp(NPC.Center, spawnPoint, AttackTimer / 60f).Y);
                flail.NPC.Center = Vector2.SmoothStep(flail.NPC.Center, NPC.Center, AttackTimer / 60f);
            }

            if (AttackTimer == 61)
            {
                NPC.TargetClosest();
                targetPoint = Main.player[NPC.target].Center;
            }

            float size = Vector2.Distance(targetPoint, NPC.Center);
            if (size > 400) size = 400;

            if (AttackTimer > 61)
            {
                //following in X direction only
                Player Player = Main.player[NPC.target];

                if (Player.Center.X > NPC.Center.X) NPC.velocity.X += 0.2f;
                else NPC.velocity.X -= 0.2f;

                if (NPC.velocity.LengthSquared() > 16) NPC.velocity = Vector2.Normalize(NPC.velocity) * 4;

                //movement of the flail
                float progress = 0;
                if (AttackTimer < 180) progress = (AttackTimer - 60) / 120f;
                if (AttackTimer >= 180 && AttackTimer < 400) progress = 1;
                if (AttackTimer >= 400) progress = (60 - (AttackTimer - 400)) / 60f;

                float rot = 3.14f + (AttackTimer - 60) / 400f * 6.28f * 6;
                Vector2 target = Vector2.SmoothStep(NPC.Center, NPC.Center + Vector2.UnitY.RotatedBy(rot) * size, progress);
                flail.NPC.Center = target;

                //dust
                for (int k = 0; k < 3; k++) Dust.NewDust(flail.NPC.position, flail.NPC.width, flail.NPC.height, DustType<Content.Dusts.GoldWithMovement>());
                for (int k = 0; k < 8; k++) Dust.NewDustPerfect(Vector2.Lerp(flail.NPC.Center, flail.NPC.oldPosition + flail.NPC.Size / 2, k / 8f), DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * 0.5f);
            }

            if (AttackTimer > 400) //deceleration
            {
                float length = 4 - (AttackTimer - 400) / 90f * 4;
                if (NPC.velocity.LengthSquared() > length * length) NPC.velocity = Vector2.Normalize(NPC.velocity) * length;
            }

            if (AttackTimer >= 490) ResetAttack();
        }

        private void Phase1Bolts()
        {
            Vector2 handpos = NPC.Center; //used as a basepoint for this attack to match the animation

            if (AttackTimer <= 30)
            {
                float rot = Main.rand.NextFloat(6.28f); //random rotation for the dust
                Dust.NewDustPerfect(handpos + Vector2.One.RotatedBy(rot) * 50, DustType<Content.Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 2); //"suck in" charging effect
            }

            if (AttackTimer == 30)
            {
                RandomizeTarget(); //pick a random target
                if (Main.player[NPC.target] == null) //safety check
                {
                    ResetAttack();
                    return;
                }
            }

            if (AttackTimer == 60) targetPoint = Main.player[NPC.target].Center;

            if (AttackTimer >= 60 && AttackTimer <= 120 && AttackTimer % 30 == 0) //3 rounds of Projectiles
            {
                Terraria.Audio.SoundEngine.PlaySound(Mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ProjectileLaunch1"), NPC.Center);
                for (float k = -0.6f; k <= 0.6f; k += 0.3f) //5 Projectiles in even spread
                {
                    Vector2 trajectory = Vector2.Normalize(targetPoint - handpos).RotatedBy(k + (AttackTimer == 90 ? 0.15f : 0)) * 1.6f; //towards the target, alternates on the second round
                    Projectile.NewProjectile(handpos, trajectory, ProjectileType<Phase1Bolt>(), 20, 0.2f); //TODO: Relocate or delete that Projectile ,or whatever this whole fight is prolly gonna get redone later anyways
                }
            }

            if (AttackTimer == 200) ResetAttack();
        }

        private void Phase1Toss()
        {
            if (AttackTimer == 1)
            {
                NPC.TargetClosest();
                NPC.velocity.X = Main.player[NPC.target].Center.X > NPC.Center.X ? 16 : -16;
            }

            if (AttackTimer <= 60)
            {
                if (NPC.velocity.X > 0 && NPC.Center.X > Main.player[NPC.target].Center.X) NPC.velocity.X *= 0.9f;
                if (NPC.velocity.X < 0 && NPC.Center.X < Main.player[NPC.target].Center.X) NPC.velocity.X *= 0.9f;

                flail.NPC.Center = Vector2.Lerp(flail.NPC.Center, NPC.Center, AttackTimer / 60);
            }

            if (AttackTimer == 60)
            {
                NPC.velocity *= 0;

                if (Main.player[NPC.target] == null) { ResetAttack(); return; } //safety check

                targetPoint = Main.player[NPC.target].Center + Main.player[NPC.target].velocity * 30; //sets the target to the closest Player

                if (Vector2.Distance(Main.player[NPC.target].Center, targetPoint) > 300)
                    targetPoint = Main.player[NPC.target].Center + Vector2.Normalize(Main.player[NPC.target].Center + targetPoint) * 300; //clamp to 300 pixels away
            }

            Vector2 trajectory = -Vector2.Normalize(flail.NPC.Center - targetPoint); //boss' toss direction

            if (AttackTimer > 60 && AttackTimer < 120)
            {
                float time = AttackTimer - 60;
                float rot = 0.418667f * time - 0.00348889f * (float)Math.Pow(time, 2); //quadratic regression of {0, 0}, {60, 12.56}, {120, 0} over the range [0, 60]

                flail.NPC.Center = NPC.Center + -Vector2.UnitY.RotatedBy(rot) * (AttackTimer - 60) * 1.8f; //spinup animation
            }

            if (AttackTimer >= 120 && AttackTimer < 130) flail.NPC.velocity += trajectory * 2; //accelerate the flail towards it's intended path

            if ((flail.NPC.velocity.Y == 0 || flail.NPC.velocity.X == 0 || Main.tile[(int)flail.NPC.Center.X / 16, (int)flail.NPC.Center.Y / 16 + 1].collisionType == 1) && !(flail.NPC.velocity.Y == 0 && flail.NPC.velocity.X == 0)) //hit the ground
            {
                //updates
                flail.NPC.velocity *= 0;
                AttackTimer = 180;

                //adds
                for (int k = 0; k < 3; k++)
                    NPC.NewNPC((int)flail.NPC.Center.X + Main.rand.Next(-100, 100), (int)flail.NPC.Center.Y, NPCType<SkeletonMinion>());

                //visuals
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(flail.NPC.position, flail.NPC.width, flail.NPC.height, DustType<Dusts.Stone>(), Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                    Dust.NewDustPerfect(flail.NPC.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, default, 1);
                }

                //audio
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, flail.NPC.Center);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, flail.NPC.Center);

                //screenshake
                int distance = (int)Vector2.Distance(Main.LocalPlayer.Center, flail.NPC.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += distance < 500 ? (500 - distance) / 15 : 20;
            }

            if (AttackTimer == 200) ResetAttack();
        }

        private void DrawTossTell(SpriteBatch sb)
        {
            float glow = AttackTimer > 90 ? 1 - (AttackTimer - 90) / 30f : (AttackTimer - 60) / 30f;
            Color color = new Color(255, 70, 70) * glow;
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Gores/TellBeam").Value;
            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            for (float k = 0; 1 == 1; k++)
            {
                Vector2 start = NPC.Center;
                Vector2 point = Vector2.Lerp(start, start + Vector2.Normalize(targetPoint - NPC.Center) * tex.Frame().Width, k);
                sb.Draw(tex, point - Main.screenPosition, tex.Frame(), color, (targetPoint - NPC.Center).ToRotation(), tex.Frame().Size() / 2, 1, 0, 0);

                if (!WorldGen.InWorld((int)point.X / 16, (int)point.Y / 16)) break;
                Tile tile = Framing.GetTileSafely(point / 16);
                if (tile.HasTile) break;
            }
            sb.End();
            sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }

        private void Phase1Trap()
        {
            if (AttackTimer == 1)
            {
                RandomizeTarget();
                targetPoint = Main.player[NPC.target].Center + new Vector2(0, -50);
            }

            if (AttackTimer == 90)
            {
                foreach (Player Player in Main.player.Where(p => p.active && Helper.CheckCircularCollision(targetPoint, 100, p.Hitbox))) //circular collision
                    Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " was strangled..."), 50, 0); //hurt em

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
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Gores/TellCircle").Value;

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
            Player Player = Main.player[NPC.target];

            if (Player.Center.X > NPC.Center.X) NPC.velocity.X += 0.3f;
            else NPC.velocity.X -= 0.3f;

            if (NPC.velocity.LengthSquared() > 25) NPC.velocity = Vector2.Normalize(NPC.velocity) * 5;
            NPC.velocity.Y = (float)Math.Sin(GlobalTimer / 100f * 6.283f) * 2;

            //attack
            if (AttackTimer <= 15)
                flail.NPC.Center = Vector2.Lerp(flail.NPC.Center, NPC.Center, AttackTimer / 15);
            if (AttackTimer == 15)
            {
                NPC.TargetClosest();
                targetPoint = Main.player[NPC.target].Center + Main.player[NPC.target].velocity * 10; //sets the target to the closest Player
                if (Vector2.Distance(Main.player[NPC.target].Center, targetPoint) > 300) targetPoint = Main.player[NPC.target].Center + Vector2.Normalize(Main.player[NPC.target].Center + targetPoint) * 300; //clamp to 3d00 pixels away
            }

            Vector2 trajectory = -Vector2.Normalize(flail.NPC.Center - targetPoint); //boss' toss direction

            if (AttackTimer > 15 && AttackTimer < 45)
            {
                float time = 2 * (AttackTimer - 15);
                float rot = 0.418667f * time - 0.00348889f * (float)Math.Pow(time, 2); //quadratic regression of {0, 0}, {60, 12.56}, {120, 0} over the range [0, 60]

                flail.NPC.Center = NPC.Center + -Vector2.UnitY.RotatedBy(rot) * (AttackTimer - 15) * 2.8f; //spinup animation
            }

            if (AttackTimer >= 45 && AttackTimer < 55) flail.NPC.velocity += trajectory * 2.4f;

            if ((flail.NPC.velocity.Y == 0 || flail.NPC.velocity.X == 0 || Main.tile[(int)flail.NPC.Center.X / 16, (int)flail.NPC.Center.Y / 16 + 1].collisionType == 1) && !(flail.NPC.velocity.Y == 0 && flail.NPC.velocity.X == 0)) //hit the ground
            {
                //updates
                flail.NPC.velocity *= 0;
                AttackTimer = 85;

                //visuals
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(flail.NPC.position, flail.NPC.width, flail.NPC.height, DustType<Dusts.Stone>(), Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                    Dust.NewDustPerfect(flail.NPC.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, default, 1);
                }

                //audio
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, flail.NPC.Center);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, flail.NPC.Center);

                //screenshake
                int distance = (int)Vector2.Distance(Main.LocalPlayer.Center, flail.NPC.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += distance < 100 ? distance / 20 : 5;
            }

            if (AttackTimer >= 95) ResetAttack();
        }

        private void DrawRapidTossTell(SpriteBatch sb)
        {
            float glow = AttackTimer < 15 ? AttackTimer / 15f : 1 - (AttackTimer - 15) / 15f;
            Color color = new Color(255, 70, 70) * glow;
            Texture2D tex = Request<Texture2D>(AssetDirectory.MiscTextures + "TellBeam").Value;

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            for (float k = 0; 1 == 1; k++)
            {
                Vector2 start = NPC.Center;
                Vector2 point = Vector2.Lerp(start, start + Vector2.Normalize(targetPoint - NPC.Center) * tex.Frame().Width, k);
                sb.Draw(tex, point - Main.screenPosition, tex.Frame(), color, (targetPoint - NPC.Center).ToRotation(), tex.Frame().Size() / 2, 1, 0, 0);

                if (!WorldGen.InWorld((int)point.X / 16, (int)point.Y / 16)) break;
                Tile tile = Framing.GetTileSafely(point / 16);
                if (tile.HasTile) break;
            }
            sb.End();
            sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
        #endregion
    }
}