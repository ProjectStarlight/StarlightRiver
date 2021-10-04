using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Breacher
{
    [AutoloadEquip(EquipType.Head)]
    public class BreacherHead : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherHead";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Visor");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class BreacherChest : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherChest";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Chestplate");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
        }


        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<BreacherHead>() && legs.type == ModContent.ItemType<BreacherLegs>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "A breacher drone follows you \nDouble tap down to call an airstrike on a nearby enemy";
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SpotterDrone>()] < 1)
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<SpotterDrone>(), (int)(50 * player.rangedDamage), 1.5f, player.whoAmI);
            }
            player.GetModPlayer<BreacherPlayer>().SetBonusActive = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class BreacherLegs : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + "BreacherLegs";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Leggings");
            Tooltip.SetDefault("Add stats later");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
        }
    }

    public class SpotterDrone : ModProjectile
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public int ScanTimer = 0;

        public const int ScanTime = 230;

        public bool CanScan => ScanTimer <= 0;

        public int Charges = 0;

        float timer = 0;

        const float attackRange = 200;

        private NPC target;

        private Vector2 enemyOffset = Vector2.Zero;

        private int attackDelay;

        private List<float> rotations;
        private List<float> rotations2;

        private float CurrentRotation => (targetPos - projectile.Center).ToRotation();
        private float CurrentRotation2 => (targetPos2 - projectile.Center).ToRotation();

        private int targetHeight
        {
            get
            {
                if (target == null || !target.active)
                    return 0;
                else
                    return (int)(target.height * 2.5f);
            }
        }

        private Vector2 targetPos => Vector2.Lerp(target.Bottom, target.Top, 0.5f + ((float)Math.Cos(((ScanTimer - 100) * 2) * 6.28f / (float)(ScanTime - 100)) / 2f));
        private Vector2 targetPos2 => Vector2.Lerp(target.Top, target.Bottom, 0.5f + ((float)Math.Cos(((ScanTimer - 100) * 2) * 6.28f / (float)(ScanTime - 100)) / 2f));
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breacher Drone");
            Main.projPet[projectile.type] = true;
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.netImportant = true;
            projectile.width = 20;
            projectile.height = 20;
            projectile.friendly = false;
            projectile.minion = true;
            projectile.minionSlots = 0;
            projectile.penetrate = -1;
            projectile.timeLeft = 216000;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            timer += 0.1f;
            if (player.GetModPlayer<BreacherPlayer>().SetBonusActive)
                projectile.timeLeft = 2;
            if (ScanTimer <= 0)
            {
                if (player.GetModPlayer<BreacherPlayer>().ticks < BreacherPlayer.CHARGETIME * 5)
                    player.GetModPlayer<BreacherPlayer>().ticks++;
                IdleMovement(player);
                Vector2 direction = player.Center - projectile.Center;
                projectile.rotation = direction.ToRotation() + 3.14f;
            }
            else
                AttackBehavior(player);
        }

        private void IdleMovement(Entity entity)
        {
            Vector2 toEntity = (entity.Center - new Vector2((entity.width + 50) * entity.direction, entity.height + 25)) - projectile.Center;
            if (toEntity.Length() > 1000)
                projectile.Center = (entity.Center - new Vector2((entity.width + 50) * entity.direction, entity.height + 25));
            toEntity.Normalize();
            toEntity *= 10;
            projectile.velocity = Vector2.Lerp(projectile.velocity, toEntity, 0.02f);
        }

        private void AttackMovement(Entity entity)
        {
            Vector2 toEntity = (entity.Center + enemyOffset) - projectile.Center;
            toEntity.Normalize();
            toEntity *= 15;
            projectile.velocity = Vector2.Lerp(projectile.velocity, toEntity, 0.06f);
            if (ScanTimer % 40 == 0)
                enemyOffset = new Vector2(Main.rand.Next(-entity.width * 2, entity.width * 2), Main.rand.Next(-entity.height * 2, 0));
        }

        private void AttackBehavior(Player player)
        {
            if (ScanTimer == ScanTime)
            {
                NPC testtarget = Main.npc.Where(n => n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, Main.MouseWorld)).FirstOrDefault();

                if (testtarget != default)
                {
                    if (Vector2.Distance(testtarget.Center, projectile.Center) < attackRange)
                    {
                        Helper.PlayPitched("Effects/Scan", 0.5f, 0);
                        target = testtarget;
                        ScanTimer--;
                        rotations = new List<float>();
                        rotations2 = new List<float>();
                        target.GetGlobalNPC<BreacherGNPC>().Targetted = true;
                    }
                    else
                    {
                        AttackMovement(testtarget);
                    }
                }
                else
                    IdleMovement(player);
            }
            else 
            {
                if (!target.active || target == null)
                {
                    ScanTimer = ScanTime;
                    return;
                }
                if (ScanTimer > Charges)
                {
                    if (rotations == null)
                    {
                        rotations = new List<float>();
                        rotations2 = new List<float>();
                    }
                    rotations.Add(CurrentRotation);
                    while (rotations.Count > 8)
                        rotations.RemoveAt(0);

                    rotations2.Add(CurrentRotation2);
                    while (rotations2.Count > 8)
                        rotations2.RemoveAt(0);

                    if (ScanTimer < 150)
                        player.GetModPlayer<StarlightPlayer>().Shake = (int)MathHelper.Lerp(0, 2, 1 - ((float)ScanTimer / 150f));
                    if (ScanTimer == 125)
                        Helper.PlayPitched("AirstrikeIncoming", 0.6f, 0);
                    ScanTimer--;
                }
                else
                {
                    if (attackDelay == 0)
                        SummonStrike();
                    attackDelay--;
                }
                if (ScanTimer == 100)
                    Helper.PlayPitched("Effects/ScanComplete", 0.5f, 0);
                if (ScanTimer > 100)
                {
                    target.GetGlobalNPC<BreacherGNPC>().Targetted = true;
                    AttackMovement(target);
                    Vector2 direction = targetPos - projectile.Center;
                    projectile.rotation = direction.ToRotation() + 3.14f;
                }
                else
                {
                    target.GetGlobalNPC<BreacherGNPC>().Targetted = false;
                    IdleMovement(player);
                    Vector2 direction = player.Center - projectile.Center;
                    projectile.rotation = direction.ToRotation() + 3.14f;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            if (target == null || !target.active)
                return;
            target.GetGlobalNPC<BreacherGNPC>().Targetted = false;
        }

        private void SummonStrike()
        {
            attackDelay = 6;
            Vector2 direction = new Vector2(0, -1);
            direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            Projectile.NewProjectile(target.Center + (direction * 800), direction * -10, ModContent.ProjectileType<OrbitalStrike>(), projectile.damage, projectile.knockBack, projectile.owner, target.whoAmI);
            Charges--;
        }
        /*public void DrawPrimitives()
        {
            if (ScanTimer == ScanTime || ScanTimer <= 100)
                return;

            Vector2[] target = new Vector2[3];
            Vector2[] source = new Vector2[3];
            if (projectile.Center.X < targetPos.X)
            {
                target[0] = projectile.Center - Main.screenPosition;
                source[0] = Vector2.Zero * 32;

                target[1] = targetPos - Main.screenPosition;
                source[1] = new Vector2(1, 1) * 32;

                target[2] = new Vector2(Vector2.Lerp(projectile.Center, targetPos, 0.5f).X, targetPos.Y) - Main.screenPosition;
                source[2] = new Vector2(0.5f, 1) * 32;
            }
            else
            {
                target[0] = projectile.Center - Main.screenPosition;
                source[0] = new Vector2(1, 0) * 32;

                target[2] = targetPos - Main.screenPosition;
                source[2] = new Vector2(0, 1) * 32;

                target[1] = new Vector2(Vector2.Lerp(projectile.Center, targetPos, 0.5f).X, targetPos.Y) - Main.screenPosition;
                source[1] = new Vector2(0.5f, 1) * 32;
            }

            Texture2D tex = ModContent.GetTexture("StarlightRiver/Assets/Items/Breacher/BreacherLaserPixel");

            DrawHelper.DrawTriangle(tex, target, source);
        }*/

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (ScanTimer == ScanTime || ScanTimer <= 100 || rotations == null || rotations.Count < 2 || rotations2.Count < 2)
                return true;

            Color color = Color.Lerp(new Color(255,0,0), Color.Red, 0.65f);
            color.A = 0;

            float oldRot = rotations[0];
            float currentRotation = rotations[Math.Max(rotations.Count - 1, 0)];
            float rotDifference = ((((currentRotation - oldRot) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            spriteBatch.Draw(Main.magicPixel, projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, currentRotation, Vector2.Zero, new Vector2((targetPos - projectile.Center).Length(), 2), SpriteEffects.None, 0);
            if (rotDifference > 0)
            {
                for (float k = 0; k < rotDifference; k += 0.02f * Math.Sign(rotDifference))
                    DrawLine(spriteBatch, k, oldRot, currentRotation, rotDifference, targetPos);
            }
            else
            {
                for (float k = 0; k > rotDifference; k += 0.02f * Math.Sign(rotDifference))
                    DrawLine(spriteBatch, k, oldRot, currentRotation, rotDifference, targetPos);
            }
            //-----------------------------------------------------------------------//
            oldRot = rotations2[0];
            currentRotation = rotations2[Math.Max(rotations2.Count - 1, 0)];
            rotDifference = ((((currentRotation - oldRot) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            spriteBatch.Draw(Main.magicPixel, projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, currentRotation, Vector2.Zero, new Vector2((targetPos2 - projectile.Center).Length(), 2), SpriteEffects.None, 0);
            if (rotDifference > 0)
            {
                for (float k = 0; k < rotDifference; k += 0.01f * Math.Sign(rotDifference))
                    DrawLine(spriteBatch, k, oldRot, currentRotation, rotDifference, targetPos2);
            }
            else
            {
                for (float k = 0; k > rotDifference; k += 0.01f * Math.Sign(rotDifference))
                    DrawLine(spriteBatch, k, oldRot, currentRotation, rotDifference, targetPos2);
            }
            return true;
        }

        private void DrawLine(SpriteBatch spriteBatch, float k, float oldRot, float currentRotation, float rotDifference, Vector2 targetPosition)
        {
            float rot = k + oldRot;
            float lerper = Math.Abs(k / rotDifference);
            lerper *= lerper * lerper;
            Color color = Color.Lerp(Color.Red, new Color(255, 0, 0), (lerper * lerper) / 2);
            color.A = 0;
            spriteBatch.Draw(Main.magicPixel, projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), color* lerper * 0.5f, rot, Vector2.Zero, new Vector2((targetPosition - projectile.Center).Length(), 2), SpriteEffects.None, 0);
        }
    }
    internal class OrbitalStrike : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        private bool hit = false;

        private NPC target => Main.npc[(int)projectile.ai[0]];

        private float Alpha => hit ? (projectile.timeLeft / 50f) : 1;
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 20;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = 1;
            projectile.timeLeft = 300;
            projectile.extraUpdates = 4;
            projectile.scale = 0.6f;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbital Strike");
            Main.projFrames[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }
        public override void AI()
        {
            if (!hit)
            {
                Vector2 direction = target.Center - projectile.Center;
                direction.Normalize();
                direction *= 10;
                if (direction.Y > 0)
                    projectile.velocity = direction;
                ManageCaches();
            }
            ManageTrail();
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 100; i++)
                {
                    cache.Add(projectile.Center);
                }
            }
            cache.Add(projectile.oldPos[0] + new Vector2(projectile.width / 2, projectile.height / 2));

            while (cache.Count > 100)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(11, 22, factor), factor =>
            {
                return Color.Cyan;
            });
            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(6, 12, factor), factor =>
            {
                return Color.White;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = projectile.Center;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["alpha"].SetValue(Alpha);

            trail?.Render(effect);

            trail2?.Render(effect);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            Color color = Color.Cyan;
            color.A = 0;
            Color color2 = Color.White;
            color2.A = 0;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null,
                             color * Alpha * 0.33f, projectile.rotation, tex.Size() / 2, projectile.scale * 2, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null,
                             color * Alpha, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null,
                             color2 * Alpha, projectile.rotation, tex.Size() / 2, projectile.scale * 0.75f, SpriteEffects.None, 0);
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake += 9;
            projectile.friendly = false;
            projectile.penetrate++;
            hit = true;
            projectile.timeLeft = 50;
            projectile.extraUpdates = 3;
            projectile.velocity = Vector2.Zero;

            Explode(target);
        }
        private void Explode(NPC target)
        {
            Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(projectile.Center + new Vector2(20, 70), ModContent.DustType<BreacherDustThree>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(12, 26), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.7f, 0.9f));
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<BreacherDustTwo>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(8), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.1f, 0.2f));
            }
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<OrbitalStrikeRing>(), projectile.damage, projectile.knockBack, projectile.owner, target.whoAmI);
        }
    }
    internal class OrbitalStrikeRing : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;
        public override string Texture => AssetDirectory.BreacherItem + "OrbitalStrike";

        private float Progress => 1 - (projectile.timeLeft / 10f);

        private float Radius => 66 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbital Strike");
        }

        public override void AI()
        {

            ManageCaches();
            ManageTrail();
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;
            for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
            {
                return Color.Cyan;
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 33f / 32f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = projectile.Center + offset;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == (int)projectile.ai[0])
                return false;
            return base.CanHitNPC(target);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + line))
            {
                return true;
            }
            return false;
        }
    }

    public class BreacherGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool Targetted = false;
    }
    public class BreacherPlayer : ModPlayer
    {
        public const int CHARGETIME = 150;

        public int ticks;
        public int Charges => ticks / CHARGETIME;

        public bool SetBonusActive;

        public override void ResetEffects()
        {
            SetBonusActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && ticks < CHARGETIME * 5)
                ticks += CHARGETIME / 3;
            base.OnHitNPCWithProj(proj, target, damage, knockback, crit);
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && ticks < CHARGETIME * 5)
                ticks += CHARGETIME / 3;
            base.OnHitNPC(item, target, damage, knockback, crit);
        }
    }
    public class BreacherArmorHelper : ILoadable
    {
        public static RenderTarget2D npcTarget;

        public static bool antiRecursion = false;
        public float Priority { get => 1.1f; }

        public void Load()
        {
            if (Main.dedServ)
                return;

            ResizeTarget();

            Main.OnPreDraw += Main_OnPreDraw;
            On.Terraria.Main.DrawNPC += Main_DrawNPC;
        }

        public static void ResizeTarget()
        {
            npcTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }

        public void Unload()
        {
            Main.OnPreDraw -= Main_OnPreDraw;
            On.Terraria.Main.DrawNPC -= Main_DrawNPC;
        }
        private static void Main_DrawNPC(On.Terraria.Main.orig_DrawNPC orig, Main self, int i, bool behindTiles)
        {
            if (!Main.npc[i].GetGlobalNPC<BreacherGNPC>().Targetted || !antiRecursion)
                orig(self, i, behindTiles);
            if (antiRecursion)
                DrawNPCTarget(i);
        }
        private void Main_OnPreDraw(GameTime obj)
        {
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || npcTarget == null || gD == null)
                return;

            antiRecursion = false;
            RenderTargetBinding[] bindings = gD.GetRenderTargets();
            gD.SetRenderTarget(npcTarget);
            gD.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.GetGlobalNPC<BreacherGNPC>().Targetted)
                {
                    if (npc.modNPC != null)
                    {
                        if (npc.modNPC != null && npc.modNPC is ModNPC modNPC)
                        {
                            if (modNPC.PreDraw(spriteBatch, npc.GetAlpha(Color.White)))
                            {
                                Main.instance.DrawNPC(i, false);
                            }
                            modNPC.PostDraw(spriteBatch, npc.GetAlpha(Color.White));
                        }
                    }
                    else
                    {
                        Main.instance.DrawNPC(i, false);
                    }
                }
            }
            spriteBatch.End();
            antiRecursion = true;
            gD.SetRenderTargets(bindings);
        }
        private static void DrawNPCTarget(int i)
        {
            NPC npc = Main.npc[i];
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || npcTarget == null || gD == null)
                return;

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            Effect effect = Filters.Scene["BreacherScan"].GetShader().Shader;
            effect.Parameters["uTime"].SetValue(1);
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(npcTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

        }
    }
}