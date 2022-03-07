using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
    public class Datsuzei : InworldItem, ILoadable
    {
        public static int activationTimer = 0; //static since this is clientside only and there really shouldnt ever be more than one of these in that context
        public int comboState = 0;

        public static ParticleSystem sparkles;

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override bool VisibleInUI => false;

        public float Priority => 1f;

        public override bool Autoload(ref string name)
		{
            return true;
		}

        public void Load()
        {
            StarlightPlayer.PostUpdateEvent += PlayerFrame;
            On.Terraria.Main.DrawInterface_30_Hotbar += OverrideHotbar;
            activationTimer = 0;
            sparkles = new ParticleSystem(AssetDirectory.Dust + "Aurora", updateSparkles);
    }

        public void Unload()
        {
            StarlightPlayer.PostUpdateEvent -= PlayerFrame;
            On.Terraria.Main.DrawInterface_30_Hotbar -= OverrideHotbar;
            sparkles = null;
        }

        public override void SetStaticDefaults()
		{
            DisplayName.SetDefault("Datsuzei");
            Tooltip.SetDefault("Unleash the moon");
		}

		public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 50;
            item.width = 16;
            item.height = 16;
            item.useStyle = ItemUseStyleID.Stabbing;
            item.useTime = 20;
            item.useAnimation = 20;
            item.shoot = ProjectileType<DatsuzeiProjectile>();
            item.shootSpeed = 20;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.crit = 10;
        }

        private void OverrideHotbar(On.Terraria.Main.orig_DrawInterface_30_Hotbar orig, Main self)
        {
            orig(self);

            if (Main.LocalPlayer.HeldItem.type != ItemType<Datsuzei>())
            {
                if (activationTimer > 0)
                    activationTimer -= 2;
                else
                {
                    activationTimer = 0;
                    sparkles.ClearParticles();
                }
            }

            if (activationTimer > 0 && !Main.playerInventory)
            {
                var activationTimerNoCurve = Datsuzei.activationTimer;
                var activationTimer = Helper.BezierEase(Math.Min(1, activationTimerNoCurve / 60f));

                var hideTarget = new Rectangle(20, 20, 446, 52);

                if(!Main.screenTarget.IsDisposed)
                    Main.spriteBatch.Draw(Main.screenTarget, hideTarget, hideTarget, Color.White * activationTimer);

                var backTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbar");
                var target = new Rectangle(111, 20, (int)(backTex.Width * activationTimer), backTex.Height);
                var source = new Rectangle(0, 0, (int)(backTex.Width * activationTimer), backTex.Height);

                Main.spriteBatch.Draw(backTex, target, source, Color.White);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

                var glowTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarGlow");

                var glowColor = new Color(200, (byte)(255 - 100 * activationTimer), 255);

                if (activationTimerNoCurve > 50)
                    glowColor *= (60 - activationTimerNoCurve) / 10f;

                if (activationTimerNoCurve < 10)
                    glowColor *= activationTimerNoCurve / 10f;

                Main.spriteBatch.Draw(glowTex, target.TopLeft() + new Vector2(target.Width, backTex.Height / 2), null, glowColor, 0, glowTex.Size() / 2, 1, 0, 0);

                if (activationTimer >= 1)
                {
                    var glowTex2 = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarGlow2");
                    var glowColor2 = new Color(200, (byte)(200 - 50 * (float)Math.Sin(Main.GameUpdateCount * 0.05f)), 255) * (Math.Min(1, (activationTimerNoCurve - 60) / 60f));

                    Main.spriteBatch.Draw(glowTex2, target.Center.ToVector2() + Vector2.UnitY * -1, null, glowColor2 * 0.8f, 0, glowTex2.Size() / 2, 1, 0, 0);
                }

                sparkles.DrawParticles(Main.spriteBatch);

                //the shader for the flames
                var effect1 = Terraria.Graphics.Effects.Filters.Scene["MagicalFlames"].GetShader().Shader;
                effect1.Parameters["sampleTexture1"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1"));
                effect1.Parameters["sampleTexture2"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2"));
                effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.008f);

                if (activationTimerNoCurve > 85)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(default, default, default, default, default, effect1, Main.UIScaleMatrix);

                    var spearTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarSprite");
                    Main.spriteBatch.Draw(spearTex, target.Center() + new Vector2(0, -40), null, Color.White, 0, spearTex.Size() / 2, 1, 0, 0);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

                if (activationTimerNoCurve >= 80)
                {
                    int overlayTime = activationTimerNoCurve - 80;
                    float overlayAlpha = 1;

                    if (overlayTime < 5)
                        overlayAlpha = 1 - overlayTime / 5f;

                    else if (overlayTime <= 25)
                        overlayAlpha = 0;

                    else if (overlayTime > 25)
                        overlayAlpha = (overlayTime - 25) / 15f;

                    else
                        overlayAlpha = 1;

                    var spearShapeTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarSpriteShape");
                    Main.spriteBatch.Draw(spearShapeTex, target.Center() + new Vector2(0, -40), null, Color.White * (1 - overlayAlpha), 0, spearShapeTex.Size() / 2, 1, 0, 0);
                }

                //particles!
                if (activationTimer < 1)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        sparkles.AddParticle(
                            new Particle(
                                new Vector2(111 + backTex.Width * activationTimer, 20 + Main.rand.Next(backTex.Height)),
                                new Vector2(Main.rand.NextFloat(-0.6f, -0.3f), Main.rand.NextFloat(3f)),
                                Main.rand.NextFloat(6.28f),
                                1,
                                Color.White,
                                60,
                                new Vector2(Main.rand.NextFloat(0.15f, 0.2f), Main.rand.NextFloat(6.28f)),
                                new Rectangle(0, 0, 100, 100)));
                    }
                }

                if (Main.rand.Next(4) == 0)
                    sparkles.AddParticle(new Particle(new Vector2(111, 20) + new Vector2(Main.rand.Next(backTex.Width), Main.rand.Next(backTex.Height)), new Vector2(0, Main.rand.NextFloat(0.4f)), 0, 0, new Color(255, 230, 0), 120, new Vector2(Main.rand.NextFloat(0.05f, 0.15f), 0.02f), new Rectangle(0, 0, 100, 100)));
            }
        }

        private static void updateSparkles(Particle particle)
        {
            particle.Timer--;

            if (particle.Velocity.X == 0)
            {
                particle.Scale = (float)(Math.Sin(particle.Timer / 120f * 3.14f)) * particle.StoredPosition.X;
                particle.Color = new Color(180, 100 + (byte)(particle.Timer / 120f * 155), 255) * (float)(Math.Sin(particle.Timer / 120f * 3.14f));
            }

            else
            {
                particle.Scale = particle.Timer / 60f * particle.StoredPosition.X;
                particle.Color = new Color(180, 100 + (byte)(particle.Timer / 60f * 155), 255) * (particle.Timer / 45f);
                particle.Color *= (float)(0.5f + Math.Sin(particle.Timer / 20f * 6.28f + particle.StoredPosition.Y) * 0.5f);
            }

            particle.Rotation += 0.05f;
            particle.Position += particle.Velocity;
        }

        private void PlayerFrame(Player player)
        {
            var proj = Main.projectile.FirstOrDefault(n => n.active && n.type == ProjectileType<DatsuzeiProjectile>() && n.owner == player.whoAmI);

            if (proj != null && proj.ai[0] == -1)
                player.bodyFrame = new Rectangle(0, 56 * 1, 40, 56);
        }

        public override bool CanUseItem(Player player)
		{
            return !Main.projectile.Any(n => n.active && n.type == ProjectileType<DatsuzeiProjectile>() && n.owner == player.whoAmI);
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Helper.PlayPitched("Magic/HolyCastShort", 1, comboState / 4f, player.Center);

            switch (comboState)
            {
                case 0:
                    int i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ProjectileType<DatsuzeiProjectile>(), damage, knockBack, player.whoAmI, 0, 40);
                    break;

                case 1:
                    i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ProjectileType<DatsuzeiProjectile>(), damage, knockBack, player.whoAmI, 1, 30);
                    break;

                case 2:
                    i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ProjectileType<DatsuzeiProjectile>(), damage, knockBack, player.whoAmI, 2, 30);
                    break;

                case 3:
                    i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ProjectileType<DatsuzeiProjectile>(), damage, knockBack, player.whoAmI, 3, 120);
                    break;
            }

            comboState++;
            if (comboState > 3)
                comboState = 0;

            return false;
        }

        public override void HoldItem(Player player)
		{
            if (player.whoAmI == Main.myPlayer)
            {
                if (!(player.armor[0].modItem is MoonstoneHead) || !(player.armor[0].modItem as MoonstoneHead).IsArmorSet(player))
                {
                    item.TurnToAir();
                    Main.LocalPlayer.QuickSpawnClonedItem(Main.mouseItem);
                    Main.mouseItem = new Item();
                }

                if (activationTimer < 120)
                    activationTimer++;
            }
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
            tooltips[0].overrideColor = new Color(100, 255, 255);
		}
    }

	public class DatsuzeiProjectile : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private List<Vector2> cacheBack;
        private Trail trailBack;

        private float storedRotation;
        private Vector2 storedPos;

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public ref float ComboState => ref projectile.ai[0];
        public ref float Maxtime => ref projectile.ai[1];
        public float Timer => Maxtime - projectile.timeLeft;

        private bool hasSetTimeLeft = false;

        public Player Owner => Main.player[projectile.owner];

		public override void SetDefaults()
		{
            projectile.melee = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.tileCollide = false;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.extraUpdates = 1;
		}

		public override void AI()
		{
            if (!hasSetTimeLeft)
            {
                projectile.timeLeft = (int)Maxtime;
                hasSetTimeLeft = true;
            }

            Owner.heldProj = projectile.whoAmI;

            if (ComboState != -1 && Timer % 2 == 0)
            {
                for (int k = 0; k < 3; k++)
                    Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 140 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);

                if (Main.rand.Next(2) == 0)
                {
                    var d = Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 140 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 0.8f);
                    d.customData = Main.rand.NextFloat(0.6f, 1.3f);
                }
            }

            switch (ComboState)
			{
                case -1: //spawning

                    projectile.rotation = 1.57f;

                    if(Timer == 1)
					{
                        Helper.PlayPitched("Impacts/Clink", 1, 0, projectile.Center);
                        Helper.PlayPitched("Magic/MysticCast", 1, -0.2f, projectile.Center);

                        for(int k = 0; k < 40; k++)
						{
                            float dustRot = Main.rand.NextFloat(6.28f);
                            Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(dustRot) * 64, DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(5), 0, new Color(100, 255, 255), 0.5f);
                        }
					}

                    if (Timer < 20)
                        projectile.scale = Timer / 20f;

                    if (Timer < 120)
                        projectile.Center = Owner.Center + new Vector2(0, -240);
                    else
                    {
                        projectile.Center = Owner.Center + Vector2.SmoothStep(new Vector2(0, -240), Vector2.Zero, (Timer - 120) / 40f);
                        projectile.alpha = (int)((Timer - 120) / 40f * 255);
                    }

                    if (Timer == 159)
                    {
                        for (int k = 0; k < 40; k++)
                        {
                            float dustRot = Main.rand.NextFloat(6.28f);
                            Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(dustRot) * 64, DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(2), 0, new Color(100, 255, 255), 0.5f);
                        }
                    }


                    break;

                case 0: //wide swing

                    if (Timer == 1)
                        storedRotation = projectile.velocity.ToRotation();

                    float rot = storedRotation + (-1.5f + Helper.BezierEase(Timer / 40f) * 3f);
                    projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (-30 + (float)Math.Sin(Timer / 40f * 3.14f) * 100);
                    projectile.rotation = rot;

                    break;

                case 1: //thin swing

                    if (Timer == 1)
                        storedRotation = projectile.velocity.ToRotation();

                    rot = storedRotation + (1f - Helper.BezierEase(Timer / 30f) * 2f);
                    projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(rot) * (-30 + (float)Math.Sin(Timer / 30f * 3.14f) * 100);
                    projectile.rotation = rot;

                    break;

                case 2: //stab

                    if (Timer == 1)
                        storedRotation = projectile.velocity.ToRotation();

                    projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(storedRotation) * (-120 + (float)Math.Sin(Timer / 30f * 3.14f) * 240);
                    projectile.rotation = storedRotation;

                    break;

                case 3: //spin

                    projectile.rotation += projectile.velocity.Length() / 200f * 6.28f;
                    projectile.velocity *= 0.97f;

                    if (Timer == 60)
                        storedPos = projectile.Center;

                    if (Timer > 60)
                        projectile.Center = Vector2.SmoothStep(storedPos, Owner.Center, (Timer - 60) / 60f);
                    break;
			}

            if (Timer > 1 && Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            return ComboState != -1 &&
                Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center - Vector2.UnitX.RotatedBy(projectile.rotation) * 140, projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 140);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
            Helper.PlayPitched("Magic/FireHit", 0.5f, 1f, target.Center);

            for (int k = 0; k < 40; k++)
            {
                float dustRot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(target.Center + Vector2.One.RotatedBy(dustRot) * 32, DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(2), 0, new Color(50, 50, 255), 0.3f);
            }

            for(int k = 0; k < 10; k++)
			{
                float dustRot = (target.Center - Owner.Center).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Dust.NewDustPerfect(target.Center + Vector2.UnitX.RotatedBy(dustRot) * 128, DustType<Dusts.GlowLine>(), Vector2.UnitX.RotatedBy(dustRot) * Main.rand.NextFloat(4), 0, new Color(50, 50, 255), 0.8f);
                Dust.NewDustPerfect(target.Center, DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(dustRot) * Main.rand.NextFloat(8), 0, new Color(50, 50, 255), 0.8f);
            }

            if(Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake <= 10)
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (ComboState == -1)
            {
                var tex = GetTexture(Texture);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), projectile.scale, 0, 0);

                var texShape = GetTexture(Texture + "Shape");
                float shapeOpacity = 0;

                if (Timer < 5)
                    shapeOpacity = Timer / 5f;
                else if (Timer < 25)
                    shapeOpacity = 1;
                else if (Timer < 40)
                    shapeOpacity = 1 - ((Timer - 25) / 15f);

                spriteBatch.Draw(texShape, projectile.Center - Main.screenPosition, null, Color.White * shapeOpacity, projectile.rotation, new Vector2(texShape.Width / 2, texShape.Height / 2), projectile.scale, 0, 0);
            }

            else
			{
                var tex = GetTexture(Texture + "Long");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(200, 200, 255) * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), projectile.scale, 0, 0);
            }


            return false;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 140);
                }
            }

                cache.Add(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 140);

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }

            if (cacheBack == null)
            {
                cacheBack = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cacheBack.Add(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * -120);
                }
            }

            cacheBack.Add(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * -120);

            while (cacheBack.Count > 50)
            {
                cacheBack.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => 10 + factor * 25, factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X * (float)Math.Sin(projectile.timeLeft / Maxtime * 3.14f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => 80 + 0 + factor * 0, factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f * (float)Math.Sin(projectile.timeLeft / Maxtime * 3.14f);
            });

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = projectile.Center + projectile.velocity;

            trailBack = trailBack ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => 20 + 0 + factor * 0, factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * (float)Math.Sin(projectile.timeLeft / Maxtime * 3.14f);
            });

            trailBack.Positions = cacheBack.ToArray();
            trailBack.NextPosition = projectile.Center + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            if (ComboState == -1)
                return;

            Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["repeats"].SetValue(8f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["sampleTexture2"].SetValue(GetTexture("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2"));

            trail?.Render(effect);

            if(ComboState == 3)
                trailBack?.Render(effect);

            effect.Parameters["sampleTexture2"].SetValue(Main.magicPixel);

            trail2?.Render(effect);
        }
    }
}
