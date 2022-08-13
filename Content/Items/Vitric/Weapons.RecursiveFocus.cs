using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using StarlightRiver.Physics;

namespace StarlightRiver.Content.Items.Vitric
{
    public class RecursiveFocus : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name; 

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Summons an infernal crystal\nThe infernal crystal locks onto enemies, ramping up damage overtime\nPress <right> to cause the crystal to target multiple enemies, at the cost of causing all beams to not ramp up, dealing less damage");
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.QueenSpiderStaff);
            Item.sentry = true;
            Item.damage = 14;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.knockBack = 0f;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold : 2);

            Item.UseSound = SoundID.Item25;
            Item.useStyle = ItemUseStyleID.HoldUp;

            Item.shoot = ModContent.ProjectileType<RecursiveFocusProjectile>();
            Item.shootSpeed = 1f;

            Item.useTime = Item.useAnimation = 35;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[Item.shoot] > 0)
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.type == Item.shoot && proj.owner == player.whoAmI)
                    {
                        proj.Kill();
                    }
                }
            Projectile.NewProjectileDirect(source, player.Center, velocity, type, damage, knockback, player.whoAmI).originalDamage = Item.damage;
            player.UpdateMaxTurrets();
            return false;
        }
    }

    public class RecursiveFocusProjectile : ModProjectile
    {
        public Player owner => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.VitricItem + Name;

        public int modeSwitchTransitionTimer;

        public int modeSwitchCooldown;

        public int flashTimer;

        public int trailMult = 5;

        public int trailMult2 = 3;

        public int timeSpentOnTarget;

        public int timeSpentOnMultiTargetOne;
        public int timeSpentOnMultiTargetTwo;

        public Vector2 targetCenter;

        public Vector2 multiTargetCenterOne;
        public Vector2 multiTargetCenterTwo;

        public bool rightClickMode;

        public bool foundTarget;

        public bool foundMultiTargetOne;
        public bool foundMultiTargetTwo;

        public bool flashing;

        public NPC oldTargetNPC;
        public NPC targetNPC;

        public NPC multiTargetNPCOne;
        public NPC multiTargetNPCTwo;

        private List<Vector2> cache;
        private Trail trail;

        private List<Vector2> cache2;
        private Trail trail2;

        private List<Vector2> multiCacheOne;
        private Trail multiTrailOne;

        private List<Vector2> multiCacheTwo;
        private Trail multiTrailTwo;

        private List<Vector2> multiCacheThree;
        private Trail multiTrailThree;

        private List<Vector2> multiCacheFour;
        private Trail multiTrailFour;

        public override void Load()
        {
            for (int i = 1; i < 5; i++)
            {
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + i);
            }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Infernal Crystal");

            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;

            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;

            Projectile.width = Projectile.height = 26;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true; //otherwise it hogs all iframes, making nothing else able to hit
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool PreAI()
        {
            if (targetNPC != null)
                oldTargetNPC = targetNPC;

            return true;
        }

        public override void AI()
        {
            if (modeSwitchTransitionTimer > 0)
            {
                modeSwitchTransitionTimer--;
                DoModeSwitchTransition();
            }

            if (modeSwitchCooldown > 0)
                modeSwitchCooldown--;

            if (Main.mouseRight && Main.mouseRightRelease && modeSwitchCooldown <= 0 && owner.HeldItem.type == ModContent.ItemType<RecursiveFocus>())
            {
                if (!rightClickMode)
                {
                    modeSwitchTransitionTimer = 120;
                    modeSwitchCooldown = 240;
                }
                else
                {
                    timeSpentOnTarget = 0;
                    timeSpentOnMultiTargetOne = 0;
                    timeSpentOnMultiTargetTwo = 0;
                    foundTarget = false;
                    foundMultiTargetOne = false;
                    foundMultiTargetTwo = false;
                    targetNPC = null;
                    multiTargetNPCOne = null;
                    multiTargetNPCTwo = null;
                    rightClickMode = false;
                }
            }

            if (!Main.dedServ && foundTarget)
            {
                ManageCaches();
                ManageTrail();
            }

            Vector2 idlePos = new Vector2(owner.Center.X, owner.Center.Y - 70);

            Vector2 toIdlePos = idlePos - Projectile.Center;
            if (toIdlePos.Length() < 0.0001f)
                toIdlePos = Vector2.Zero;

            else
            {
                float speed = Vector2.Distance(idlePos, Projectile.Center) * 0.2f;
                speed = Utils.Clamp(speed, 1f, 25f);
                toIdlePos.Normalize();
                toIdlePos *= speed;
            }

            Projectile.velocity = (Projectile.velocity * (45f - 1) + toIdlePos) / 45f;

            if (Vector2.Distance(Projectile.Center, idlePos) > 2000f)
            {
                Projectile.Center = idlePos;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];

                float dist = Vector2.Distance(npc.Center, Projectile.Center);

                if (dist < 1000f)
                {
                    targetCenter = npc.Center;
                    targetNPC = npc;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
                targetNPC = Projectile.FindTargetWithinRange(1000f);

            if (targetNPC != null)
            {
                if (oldTargetNPC != targetNPC)
                    timeSpentOnTarget = 0;

                if (Collision.CanHit(Projectile.Center, 1, 1, targetNPC.Center, 1, 1) && modeSwitchTransitionTimer <= 0)
                {
                    foundTarget = true;
                    targetCenter = targetNPC.Center;
                    if (timeSpentOnTarget < 720)
                        timeSpentOnTarget++;
                }

                if (!targetNPC.active || Vector2.Distance(Projectile.Center, targetCenter) > 1000f || !Collision.CanHit(Projectile.Center, 1, 1, targetNPC.Center, 1, 1) || modeSwitchTransitionTimer > 0)
                {
                    timeSpentOnTarget = 0;
                    foundTarget = false;
                }

                if (foundTarget)
                {
                    if (!rightClickMode)
                        UpdateSingleMode();

                    if (Projectile.soundDelay == 0)
                    {
                        Helper.PlayPitched("Magic/RecursiveFocusLaserLoopingAlt", 0.7f + MathHelper.Lerp(0, 0.6f, timeSpentOnTarget / 720f), Main.rand.NextFloat(-0.05f, 0.05f), Projectile.Center);
                        Projectile.soundDelay = 30;
                    }
                }
            }

            if (rightClickMode)
            {
                FindMultiTarget();

                if (foundTarget || foundMultiTargetOne || foundMultiTargetTwo)
                    UpdateMultiMode();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail(Main.spriteBatch);

            Texture2D baseTex = ModContent.Request<Texture2D>(Texture + "Base").Value;
            Texture2D crystalTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>(Texture + "_Bloom").Value;

            Texture2D crystalTexOrange = ModContent.Request<Texture2D>(Texture + "_Orange").Value;
            Texture2D baseTexOrange = ModContent.Request<Texture2D>(Texture + "Base_Orange").Value;

            Texture2D crystalTexGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            Color color = Color.Lerp(Color.Transparent, new Color(255, 150, 50), (timeSpentOnTarget / 360f) - 1f);
            color.A = 0;

            if (rightClickMode)
            {
                Color bloomColor = new Color(255, 150, 50);
                bloomColor.A = 0;
                Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 1.35f, SpriteEffects.None, 0);
            }
            else if (timeSpentOnTarget > 360)
                Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 1.35f, SpriteEffects.None, 0);


            Main.EntitySpriteDraw(baseTex, ((Projectile.Center + new Vector2(0, 20)) - Projectile.oldVelocity) - Main.screenPosition, null, Color.White, Projectile.velocity.X * 0.075f, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(crystalTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            if (foundTarget)
            {
                if (!rightClickMode)
                {
                    Main.EntitySpriteDraw(baseTexOrange, ((Projectile.Center + new Vector2(0, 20)) - Projectile.oldVelocity) - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White, timeSpentOnTarget / 720f), Projectile.velocity.X * 0.075f, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(crystalTexOrange, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White, timeSpentOnTarget / 720f), Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

                    if (timeSpentOnTarget >= 720)
                    {
                        Main.EntitySpriteDraw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.White, Color.Transparent, flashTimer <= 1 ? 0 : 1f - (flashTimer / 35f)), Projectile.rotation, crystalTexGlow.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                    }

                    if (flashing)
                    {
                        flashTimer++;
                        if (flashTimer > 35)
                        {
                            flashing = false;
                            flashTimer = 0;
                        }
                        Texture2D crystalTexWhite = ModContent.Request<Texture2D>(Texture + "_White").Value;
                        if (flashTimer > 0)
                            Main.EntitySpriteDraw(crystalTexWhite, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White, 1f - (flashTimer / 35f)), Projectile.rotation, crystalTexWhite.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                    }
                }
                Texture2D laserBloom = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
                Color color2 = new Color(255, 150, 50);
                color2.A = 0;
                Color color3 = new Color(255, 195, 135);
                color3.A = 0;

                Main.EntitySpriteDraw(laserBloom, targetCenter - Main.screenPosition, null, color2, 0f, laserBloom.Size() / 2f, 0.3f + MathHelper.Lerp(0, 0.4f, timeSpentOnTarget / 720f), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(laserBloom, targetCenter - Main.screenPosition, null, color3, 0f, laserBloom.Size() / 2f, 0.2f + MathHelper.Lerp(0, 0.4f, timeSpentOnTarget / 720f), SpriteEffects.None, 0);

                if (foundMultiTargetOne && timeSpentOnMultiTargetOne > 2)
                { 
                    Main.EntitySpriteDraw(laserBloom, multiTargetCenterOne - Main.screenPosition, null, color2, 0f, laserBloom.Size() / 2f, 0.5f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(laserBloom, multiTargetCenterOne - Main.screenPosition, null, color3, 0f, laserBloom.Size() / 2f, 0.4f, SpriteEffects.None, 0);
                }

                if (foundMultiTargetOne && timeSpentOnMultiTargetTwo > 2)
                {
                    Main.EntitySpriteDraw(laserBloom, multiTargetCenterTwo - Main.screenPosition, null, color2, 0f, laserBloom.Size() / 2f, 0.5f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(laserBloom, multiTargetCenterTwo - Main.screenPosition, null, color3, 0f, laserBloom.Size() / 2f, 0.4f, SpriteEffects.None, 0);
                }
            }

            if (modeSwitchTransitionTimer > 0)
            {
                float progress = 1f - modeSwitchTransitionTimer / 120f;
                Main.EntitySpriteDraw(baseTexOrange, ((Projectile.Center + new Vector2(0, 20)) - Projectile.oldVelocity) - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White, progress), Projectile.velocity.X * 0.075f, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(crystalTexOrange, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White, progress), Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            }

            else if (rightClickMode)
            {
                Main.EntitySpriteDraw(baseTexOrange, ((Projectile.Center + new Vector2(0, 20)) - Projectile.oldVelocity) - Main.screenPosition, null, Color.White, Projectile.velocity.X * 0.075f, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(crystalTexOrange, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

                Main.EntitySpriteDraw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, crystalTexGlow.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float useless = 0f;
            if (rightClickMode)
            {
                bool targetOne = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, targetCenter, 15, ref useless); 
                bool targetTwo = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, multiTargetCenterOne, 15, ref useless);
                bool targetThree = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, multiTargetCenterTwo, 15, ref useless);
                return ((foundTarget && targetOne && timeSpentOnTarget > 2) || (foundMultiTargetOne && targetTwo && timeSpentOnMultiTargetOne > 2) || (foundMultiTargetTwo && targetThree && timeSpentOnMultiTargetTwo > 2));
            }
            return timeSpentOnTarget > 2 && foundTarget && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, targetCenter, 15, ref useless);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (!rightClickMode)
            {
                damage = (int)(damage * (1f + (timeSpentOnTarget / 720f)));

                if (target != targetNPC)
                    damage = damage / 2;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            float scale = 0.45f + MathHelper.Lerp(0, 0.1f, timeSpentOnTarget / 720);
            if (target == targetNPC || target == multiTargetNPCOne || target == multiTargetNPCTwo)
                for (int i = 0; i < 7; i++)
                {
                    Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), scale);
                }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 5; i++)
            {
                Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, Mod.Find<ModGore>(Name + "_Gore" + i).Type, 1f).timeLeft = 90;
            }
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
        }

        public void DoModeSwitchTransition()
        {
            float angle = Main.rand.NextFloat(6.28f);
            if (Main.rand.NextBool())
                Dust.NewDustPerfect(Projectile.Center - (angle.ToRotationVector2() * (25f - MathHelper.Lerp(0, 20, 1 - modeSwitchTransitionTimer / 120f))), ModContent.DustType<Dusts.Glow>(), angle.ToRotationVector2() * 0.65f, 0, new Color(255, 150, 50), 0.2f);
            else
                Dust.NewDustPerfect(Projectile.Center - (angle.ToRotationVector2() * (25f - MathHelper.Lerp(0, 20, 1 - modeSwitchTransitionTimer / 120f))), ModContent.DustType<Dusts.Glow>(), angle.ToRotationVector2() * 0.65f, 0, new Color(255, 195, 135), 0.2f);

            if (modeSwitchTransitionTimer == 45)
            {
                Helper.PlayPitched("Magic/FireCast", 0.8f, 0, Projectile.Center);
            }

            if (modeSwitchTransitionTimer <= 0)
            {
                Helper.PlayPitched("Magic/FireHit", 0.8f, 0, Projectile.Center);
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), 0.85f);
                        else
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), 0.85f);
                    }
                }
                rightClickMode = true;
            }
        }

        public void FindMultiTarget()
        {
            if (!foundMultiTargetOne)
            {
                float range = 1000f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active && npc.CanBeChasedBy(Projectile))
                    {
                        bool othernpc = npc == targetNPC || npc == multiTargetNPCTwo;

                        if (!othernpc)
                            if (!npc.dontTakeDamage && Vector2.Distance(Projectile.Center, npc.Center) < range && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                            {
                                float closest = Projectile.Distance(npc.Center);
                                if (range > closest)
                                {
                                    multiTargetCenterOne = npc.Center;
                                    multiTargetNPCOne = npc;
                                    foundMultiTargetOne = true;
                                    range = closest;
                                }
                            }
                    }
                }
            }

            if (!foundMultiTargetTwo)
            {
                float range = 1000f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active && npc.CanBeChasedBy(Projectile))
                    {
                        bool othernpc = npc == targetNPC || npc == multiTargetNPCOne;

                        if (!othernpc)
                            if (!npc.dontTakeDamage && Vector2.Distance(Projectile.Center, npc.Center) < 1000f && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                            {
                                float closest = Projectile.Distance(npc.Center);
                                if (range > closest)
                                { 
                                    multiTargetCenterTwo = npc.Center;
                                    multiTargetNPCTwo = npc;
                                    foundMultiTargetTwo = true;
                                    range = closest;
                                }
                            }
                    }
                }        
            }
        }
        public void UpdateSingleMode()
        {
            float scale = 0.4f + MathHelper.Lerp(0, 0.1f, timeSpentOnTarget / 720f);
            Dust.NewDustPerfect(targetCenter, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), scale);

            Vector2 dustPos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
            float dustScale = MathHelper.Lerp(0.2f, 0.6f, timeSpentOnTarget / 720f);
            if (Main.rand.NextBool())
                Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), dustScale);
            else
                Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), dustScale);

            if (timeSpentOnTarget < 720)
            {
                float angle = Main.rand.NextFloat(6.28f);
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(Projectile.Center - (angle.ToRotationVector2() * (35f - MathHelper.Lerp(0, 30, timeSpentOnTarget / 720f))), ModContent.DustType<Dusts.Glow>(), angle.ToRotationVector2() * 0.5f, 0, new Color(255, 150, 50), 0.25f);
                else
                    Dust.NewDustPerfect(Projectile.Center - (angle.ToRotationVector2() * (35f - MathHelper.Lerp(0, 30, timeSpentOnTarget / 720f))), ModContent.DustType<Dusts.Glow>(), angle.ToRotationVector2() * 0.5f, 0, new Color(255, 195, 135), 0.25f);
            }

            if (timeSpentOnTarget == 240)
            {
                Helper.PlayPitched("Magic/FireHit", 0.5f, 0, Projectile.Center);
                flashing = true;
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.8f, 0, new Color(255, 150, 50), 0.6f);
                        else
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.8f, 0, new Color(255, 195, 135), 0.6f);
                    }

                    for (int i = 0; i < 45; i++)
                    {
                        Vector2 pos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.35f, 0, new Color(255, 150, 50), 0.5f);
                        else
                            Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.35f, 0, new Color(255, 195, 135), 0.5f);
                    }
                }
            }
            else if (timeSpentOnTarget == 480)
            {
                Helper.PlayPitched("Magic/FireHit", 0.75f, 0, Projectile.Center);
                flashing = true;
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 35; i++)
                    {
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), 0.85f);
                        else
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), 0.85f);
                    }

                    for (int i = 0; i < 55; i++)
                    {
                        Vector2 pos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.45f, 0, new Color(255, 150, 50), 0.65f);
                        else
                            Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.45f, 0, new Color(255, 195, 135), 0.65f);
                    }
                }
            }
            else if (timeSpentOnTarget == 719)
            {
                Helper.PlayPitched("Magic/FireHit", 1f, 0, Projectile.Center);
                flashing = true;
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), 0.95f);
                        else
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), 0.95f);
                    }

                    for (int i = 0; i < 60; i++)
                    {
                        Vector2 pos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.55f, 0, new Color(255, 150, 50), 0.75f);
                        else
                            Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.55f, 0, new Color(255, 195, 135), 0.75f);
                    }
                }
            }

            if (timeSpentOnTarget < 240)
            {
                trailMult = 4;
                trailMult2 = 2;
            }
            else if (timeSpentOnTarget < 480)
            {
                trailMult = 9;
                trailMult2 = 7;
            }
            else if (timeSpentOnTarget < 720)
            {
                trailMult = 14;
                trailMult2 = 12;
            }
            else
            {
                trailMult = 19;
                trailMult2 = 17;
            }

            if (timeSpentOnTarget == 165)
                Helper.PlayPitched("GlassMiniboss/GlassExplode", 0.75f, 0, Projectile.Center);
            else if (timeSpentOnTarget == 405)
                Helper.PlayPitched("GlassMiniboss/GlassExplode", 1f, 0, Projectile.Center);
            else if (timeSpentOnTarget == 645)
                Helper.PlayPitched("GlassMiniboss/GlassExplode", 1.25f, 0, Projectile.Center);
        }

        public void UpdateMultiMode()
        {
            if (timeSpentOnTarget > 2)
            {
                float scale = 0.4f;
                Dust.NewDustPerfect(targetCenter, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), scale);

                Vector2 dustPos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                float dustScale = 0.45f;
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), dustScale);
                else
                    Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), dustScale);
            }
            if (foundMultiTargetOne)
            {
                if (timeSpentOnMultiTargetOne > 2)
                {
                    float scale = 0.4f;
                    Dust.NewDustPerfect(multiTargetCenterOne, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), scale);

                    Vector2 dustPos = Vector2.Lerp(Projectile.Center, multiTargetCenterOne, Main.rand.NextFloat());
                    float dustScale = 0.45f;
                    if (Main.rand.NextBool())
                        Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), dustScale);
                    else
                        Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), dustScale);
                }

                multiTargetCenterOne = multiTargetNPCOne.Center;

                if (timeSpentOnMultiTargetOne < 480)
                    timeSpentOnMultiTargetOne++;

                if (multiTargetNPCOne == targetNPC || multiTargetNPCOne == multiTargetNPCTwo)
                {
                    multiTargetNPCOne = null;
                    foundMultiTargetOne = false;
                    timeSpentOnMultiTargetOne = 0;
                }
                else if (!multiTargetNPCOne.active || Vector2.Distance(Projectile.Center, multiTargetCenterOne) > 1000f || !Collision.CanHit(Projectile.Center, 1, 1, multiTargetCenterOne, 1, 1))
                {
                    foundMultiTargetOne = false;
                    timeSpentOnMultiTargetOne = 0;
                }
            }

            if (foundMultiTargetTwo)
            {
                if (timeSpentOnMultiTargetTwo > 2)
                {
                    float scale = 0.4f;
                    Dust.NewDustPerfect(multiTargetCenterTwo, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), scale);

                    Vector2 dustPos = Vector2.Lerp(Projectile.Center, multiTargetCenterTwo, Main.rand.NextFloat());
                    float dustScale = 0.45f;
                    if (Main.rand.NextBool())
                        Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), dustScale);
                    else
                        Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 195, 135), dustScale);
                }

                multiTargetCenterTwo = multiTargetNPCTwo.Center;

                if (timeSpentOnMultiTargetTwo < 480)
                    timeSpentOnMultiTargetTwo++;

                if (multiTargetNPCTwo == targetNPC || multiTargetNPCTwo == multiTargetNPCOne)
                {
                    multiTargetNPCTwo = null;
                    foundMultiTargetTwo = false;
                    timeSpentOnMultiTargetTwo = 0;
                }
                else if (!multiTargetNPCTwo.active || Vector2.Distance(Projectile.Center, multiTargetCenterTwo) > 1000f || !Collision.CanHit(Projectile.Center, 1, 1, multiTargetCenterTwo, 1, 1))
                {
                    foundMultiTargetTwo = false;
                    timeSpentOnMultiTargetTwo = 0;
                }
            }
        }
        private void ManageCaches()
        {
            cache = new List<Vector2>();
            for (int i = 0; i < 13; i++)
            {
                cache.Add(Vector2.Lerp(Projectile.Center, targetCenter, i / 13f));
            }
            cache.Add(targetCenter);

            cache2 = new List<Vector2>();
            for (int i = 0; i < 10; i++)
            {
                cache2.Add(Vector2.Lerp(Projectile.Center, targetCenter, i / 10f));
            }
            cache2.Add(targetCenter);


            multiCacheOne = new List<Vector2>();
            for (int i = 0; i < 13; i++)
            {
                multiCacheOne.Add(Vector2.Lerp(Projectile.Center, multiTargetCenterOne, i / 13f));
            }
            multiCacheOne.Add(multiTargetCenterOne);

            multiCacheThree = new List<Vector2>();
            for (int i = 0; i < 10; i++)
            {
                multiCacheThree.Add(Vector2.Lerp(Projectile.Center, multiTargetCenterOne, i / 10f));
            }
            multiCacheThree.Add(multiTargetCenterOne);


            multiCacheTwo = new List<Vector2>();
            for (int i = 0; i < 13; i++)
            {
                multiCacheTwo.Add(Vector2.Lerp(Projectile.Center, multiTargetCenterTwo, i / 13f));
            }
            multiCacheTwo.Add(multiTargetCenterTwo);

            multiCacheFour = new List<Vector2>();
            for (int i = 0; i < 10; i++)
            {
                multiCacheFour.Add(Vector2.Lerp(Projectile.Center, multiTargetCenterTwo, i / 10f));
            }
            multiCacheFour.Add(multiTargetCenterTwo);
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 14, new TriangularTip(4), factor => rightClickMode ? 13 : trailMult, factor =>
            {
                return new Color(255, 150, 50) * 0.6f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = targetCenter;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 11, new TriangularTip(4), factor => rightClickMode ? 11 : trailMult2, factor =>
            {
                return new Color(255, 195, 135) * 0.8f * factor.X;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = targetCenter;

            multiTrailOne = multiTrailOne ?? new Trail(Main.instance.GraphicsDevice, 14, new TriangularTip(4), factor => 13, factor =>
            {
                return new Color(255, 150, 50) * 0.6f * factor.X;
            });

            multiTrailOne.Positions = multiCacheOne.ToArray();
            multiTrailOne.NextPosition = multiTargetCenterOne;

            multiTrailThree = multiTrailThree ?? new Trail(Main.instance.GraphicsDevice, 11, new TriangularTip(4), factor => 11, factor =>
            {
                return new Color(255, 195, 135) * 0.8f * factor.X;
            });

            multiTrailThree.Positions = multiCacheThree.ToArray();
            multiTrailThree.NextPosition = multiTargetCenterOne;

            multiTrailTwo = multiTrailTwo ?? new Trail(Main.instance.GraphicsDevice, 14, new TriangularTip(4), factor => 13, factor =>
            {
                return new Color(255, 150, 50) * 0.6f * factor.X;
            });

            multiTrailTwo.Positions = multiCacheTwo.ToArray();
            multiTrailTwo.NextPosition = multiTargetCenterTwo;

            multiTrailFour = multiTrailFour ?? new Trail(Main.instance.GraphicsDevice, 11, new TriangularTip(4), factor => 11, factor =>
            {
                return new Color(255, 195, 135) * 0.8f * factor.X;
            });

            multiTrailFour.Positions = multiCacheFour.ToArray();
            multiTrailFour.NextPosition = multiTargetCenterTwo;
        }

        private void DrawTrail(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.03f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            if (foundTarget && timeSpentOnTarget > 2)
            {
                trail?.Render(effect);

                trail2?.Render(effect);
            }

            if (rightClickMode)
            {
                if (foundMultiTargetOne && timeSpentOnMultiTargetOne > 2)
                {
                    multiTrailOne?.Render(effect);

                    multiTrailThree?.Render(effect);
                }

                if (foundMultiTargetTwo && timeSpentOnMultiTargetTwo > 2)
                {
                    multiTrailTwo?.Render(effect);

                    multiTrailFour?.Render(effect);
                }
            }

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

            if (foundTarget && timeSpentOnTarget > 2)
            {
                trail?.Render(effect);

                trail2?.Render(effect);
            }

            if (rightClickMode)
            {
                if (foundMultiTargetOne && timeSpentOnMultiTargetOne > 2)
                {
                    multiTrailOne?.Render(effect);

                    multiTrailThree?.Render(effect);
                }

                if (foundMultiTargetTwo && timeSpentOnMultiTargetTwo > 2)
                {
                    multiTrailTwo?.Render(effect);

                    multiTrailFour?.Render(effect);
                }
            }

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.02f);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            if (foundTarget && timeSpentOnTarget >= 720)
            {
                trail?.Render(effect);

                trail2?.Render(effect);
            }

            if (rightClickMode)
            {
                if (foundMultiTargetOne && timeSpentOnMultiTargetOne >= 480)
                {
                    multiTrailOne?.Render(effect);

                    multiTrailThree?.Render(effect);
                }

                if (foundMultiTargetTwo && timeSpentOnMultiTargetTwo >= 480)
                {
                    multiTrailTwo?.Render(effect);

                    multiTrailFour?.Render(effect);
                }
            }

            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}