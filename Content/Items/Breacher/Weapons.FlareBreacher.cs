using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Breacher
{
	public class FlareBreacher : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flare Breacher");
            Tooltip.SetDefault("LMB to launch explosive flares\nRMB to launch a marker flare, calling down fury upon your enemies after a long chargeup");
        }

        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 30;
            item.useTime = 30;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item11;
            item.width = 24;
            item.height = 28;
            item.damage = 28;
            item.rare = ItemRarityID.Orange;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = false;
            item.useAmmo = AmmoID.Flare;
            item.ranged = true;
            item.shoot = ModContent.ProjectileType<ExplosiveFlare>();
            item.shootSpeed = 17;
        }

        public override void HoldItem(Player player)
        {
            if (Main.mouseRight)
                return;
            FlareBreacherPlayer modPlayer = player.GetModPlayer<FlareBreacherPlayer>();

            if (modPlayer.ticks < FlareBreacherPlayer.CHARGETIME * 5)
                modPlayer.ticks++;
            else
                modPlayer.ticks = FlareBreacherPlayer.CHARGETIME * 5;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.GetModPlayer<FlareBreacherPlayer>().Charges < 1)
                    return false;
            }
            return true;
        }


        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 2)
            {
                type = ModContent.ProjectileType<OrbitalStrikeProj>();
                speedX = speedY = 0;
                return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
            }
            else
                type = ModContent.ProjectileType<ExplosiveFlare>();
            position.Y -= 4;
            Vector2 direction = new Vector2(speedX, speedY);

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(position + (direction * 1.35f), 6, (direction.RotatedBy(Main.rand.NextFloat(-1, 1)) / 5f) * Main.rand.NextFloat());
                dust.noGravity = true;
            }

            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }
    #region LMB projectiles
    internal class ExplosiveFlare : ModProjectile
    {
        int enemyID;
        bool stuck = false;
        int explosionTimer = 100;
        Vector2 offset = Vector2.Zero;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.aiStyle = 1;
            aiType = 163;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Flare");
            Main.projFrames[projectile.type] = 2;
        }

        public override bool PreAI()
        {
            Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 0.5f);
            Vector2 direction = (projectile.rotation + 1.57f + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2();
            if (stuck)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<BreacherDust>(), direction * Main.rand.NextFloat(3, 4));
                dust.scale = 1.15f;
                dust.noGravity = true;
                NPC target = Main.npc[enemyID];
                projectile.position = target.position + offset;
                explosionTimer--;

                if (explosionTimer <= 0 || !target.active)
                    Explode(target);
                return false;
            }
            else
            {
                for (float i = 0; i < 1; i+= 0.25f)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center - (projectile.velocity * i), ModContent.DustType<BreacherDustFour>(), direction * Main.rand.NextFloat(3, 4));
                    dust.scale = 0.85f;
                    dust.noGravity = true;
                }
                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            }

            return base.PreAI();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Dust.NewDustPerfect(projectile.Center + oldVelocity, ModContent.DustType<FlareBreacherDust>(), Vector2.Zero, 60, default, 0.7f).rotation = Main.rand.NextFloat(6.28f);
            return base.OnTileCollide(oldVelocity);
        }
        private void Explode(NPC target)
        {
            target.StrikeNPC(projectile.damage, 0f, 0);
            Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake = 10;
            int numberOfProjectiles = Main.rand.Next(5, 8);
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float offsetRad = MathHelper.Lerp(0, 0.5f, (float)i / (float)numberOfProjectiles);
                Projectile.NewProjectile(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f) * target.width, Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(0 - offsetRad, offsetRad) - 1.57f) * Main.rand.NextFloat(9, 11), ModContent.ProjectileType<FlareShrapnel>(), projectile.damage, projectile.knockBack, projectile.owner, target.whoAmI);
            }

            /*for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectileDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f) * target.width, Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.7f, 0.7f) - 1.57f) * Main.rand.NextFloat(1, 2), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, projectile.owner).scale = Main.rand.NextFloat(0.65f, 0.85f);
            }*/
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(5,10);
                dust.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust.alpha = 40 + Main.rand.Next(40);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(10,20);
                dust.scale = Main.rand.NextFloat(0.75f, 1f);
                dust.alpha = 40 + Main.rand.Next(40);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }
            for (int i = 0; i < 24; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<BreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(1, 5);
                dust.scale = Main.rand.NextFloat(0.75f, 1.1f);
            }
            Gore.NewGore(projectile.position, Vector2.Zero, mod.GetGoreSlot("Assets/Items/Breacher/FlareGore"));
            projectile.active = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];
            if (target.life <= 0)
                player.GetModPlayer<FlareBreacherPlayer>().ticks += FlareBreacherPlayer.CHARGETIME;
            if (!stuck && target.life > 0)
            {
                stuck = true;
                projectile.friendly = false;
                projectile.tileCollide = false;
                enemyID = target.whoAmI;
                offset = projectile.position - target.position;
                offset -= projectile.velocity;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = Main.projectileTexture[projectile.type];
            var source = new Rectangle(0, 0, projectile.width, 16);

            if (stuck)
                source.Y += 16 * (Main.GameUpdateCount % 10 < 5 ? 1 : 0);

            Main.spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, source, lightColor, projectile.rotation, (tex.Size() / 2) * new Vector2(1,0.5f), projectile.scale, 0, 0);

            return false;
        }
    }
    internal class FlareShrapnel : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        private NPC source => Main.npc[(int)projectile.ai[0]];

        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = Main.rand.Next(50,70);
            projectile.extraUpdates = 4;
            projectile.alpha = 255;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Shrapnel");
            Main.projFrames[projectile.type] = 2;
        }
        public override void AI()
        {
            projectile.velocity *= 0.96f;
            ManageCaches();
            ManageTrail();
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 20; i++)
                {
                    cache.Add(projectile.Center);
                }
            }
                cache.Add(projectile.Center);

            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(40 * 4), factor => factor * 6, factor =>
            {
                return new Color(255, 50, 180);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["ShrapnelTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["progress"].SetValue(MathHelper.Lerp(projectile.timeLeft / 60f, 0, 0.3f));

            trail?.Render(effect);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];
            if (target.life <= 0)
                player.GetModPlayer<FlareBreacherPlayer>().ticks += FlareBreacherPlayer.CHARGETIME;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (target == source)
                return false;
            return base.CanHitNPC(target);
        }
    }
    #endregion

    #region RMB projectiles
    public class OrbitalStrikeProj : ModProjectile
    {
        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        private int charge = 0;

        private bool charged = false;

        private int Strikes => charge / 150;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbital Strike");
        }
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.ranged = true;
            projectile.width = 2;
            projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = 1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999999;
            projectile.ignoreWater = true;
            projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);

            player.itemTime = 25; 
            player.itemAnimation = 25;
            projectile.position = player.Center;

            Vector2 direction = Main.MouseWorld - (player.Center - new Vector2(0, 4));
            direction.Normalize();

            if (Main.mouseRight)
            {
                if (charge < player.GetModPlayer<FlareBreacherPlayer>().ticks)
                    charge+= 5;
                else if (!charged)
                {
                    charged = true;
                    //fully charged effects here
                }
                if (charge % 150 == 0 && !charged)
                {
                    //charge sound effect here
                }
                player.itemRotation = direction.ToRotation();
                if (player.direction != 1)
                {
                    player.itemRotation -= 3.14f;
                }
            }
            else
            {
                if (charge > 150)
                {
                    player.GetModPlayer<FlareBreacherPlayer>().ticks -= charge;
                    Projectile proj = Projectile.NewProjectileDirect(projectile.Center - new Vector2(0, 4), direction * 15, ModContent.ProjectileType<OrbitalStrikePredictor>(), projectile.damage, projectile.knockBack, projectile.owner);
                    if (proj.modProjectile is OrbitalStrikePredictor modProj)
                        modProj.Strikes = Strikes;
                }
                projectile.active = false;
            }
        }
    }

    public class OrbitalStrikePredictor : ModProjectile
    {
        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        public int Strikes;

        int enemyID;
        bool stuck = false;
        int strikeTimer = 150;
        Vector2 offset = Vector2.Zero;
        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.aiStyle = 1;
            aiType = 163;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flare");
            Main.projFrames[projectile.type] = 2;
        }
        public override bool PreAI()
        {
            Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 0.25f);
            if (stuck)
            {
                if (strikeTimer > -4)
                    Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake = (int)MathHelper.Lerp(0, 2, 1 - ((float)strikeTimer / 150f));
                NPC target = Main.npc[enemyID];
                projectile.position = target.position + offset;
                strikeTimer--;
                if (strikeTimer == 125)
                    Helper.PlayPitched("AirstrikeIncoming", 0.6f, 0);
                if (strikeTimer <= 0 && strikeTimer % 6 == 0)
                    Strike(target);
                return false;
            }
            else
                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            return base.PreAI();
        }

        private void Strike(NPC target)
        {
            Player player = Main.player[projectile.owner];

            Vector2 direction = new Vector2(0, -1);
            direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            Projectile.NewProjectile(target.Center + (direction * 800), direction * -10, ModContent.ProjectileType<OrbitalStrike>(), projectile.damage, projectile.knockBack, projectile.owner);

            Strikes--;
            if (Strikes <= 0)
                projectile.active = false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];
            if (target.life <= 0)
                player.GetModPlayer<FlareBreacherPlayer>().ticks += FlareBreacherPlayer.CHARGETIME;
            if (!stuck && target.life > 0)
            {
                stuck = true;
                projectile.friendly = false;
                projectile.tileCollide = false;
                enemyID = target.whoAmI;
                offset = projectile.position - target.position;
                offset -= projectile.velocity;
            }
        }
    }

    internal class OrbitalStrike : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        private bool hit = false;

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
                ManageCaches();
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

            Explode();
        }
        private void Explode()
        {
            Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(projectile.Center + new Vector2(20, 70), ModContent.DustType<BreacherDustThree>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(12, 26), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.7f, 0.9f));
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<BreacherDustTwo>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(8), 0, new Color(48, 242, 96), Main.rand.NextFloat(0.1f, 0.2f));
            }
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<OrbitalStrikeRing>(), projectile.damage, projectile.knockBack, projectile.owner);
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
    }
        #endregion
    public class FlareBreacherPlayer : ModPlayer
    {
        public const int CHARGETIME = 150;

        public int ticks;
        public int Charges => ticks / CHARGETIME;

        public override void ResetEffects()
        {
            //Main.NewText("Charge is " + ticks.ToString());
        }
    }
}