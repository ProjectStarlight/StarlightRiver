using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.BuriedArtifacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
    class GeodeBow : ModItem
    { 
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Bow");
            Tooltip.SetDefault("Hit enemies to create crystal growths \nShoot these growths to deal massive damage");
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.channel = true;
            Item.shoot = ProjectileType<GeodeBowProj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.useTurn = true;
            Item.channel = true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.itemTime == 2;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<GeodeBowProj>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity / 4f, ProjectileType<GeodeBowProj>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ExoticGeodeArtifactItem>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    internal class GeodeBowProj : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Bow");
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            if (!owner.channel)
                Projectile.active = false;

            owner.itemAnimation = owner.itemTime = 2;
            owner.direction = Math.Sign(owner.DirectionTo(Main.MouseWorld).X);
            Projectile.rotation = owner.DirectionTo(Main.MouseWorld).ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = owner.Center;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            owner.heldProj = Projectile.whoAmI;

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 6 == 5)
            {
                Projectile.frame++;
                if (Projectile.frame == 4)
                    Shoot();
            }

            if (Projectile.frame >= 6)
            {
                 Projectile.frame = 0;
                 Projectile.frameCounter = 0;
            }

            Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
            if (Projectile.frame == 3)
                stretch = Player.CompositeArmStretchAmount.None;
            else if (Projectile.frame == 2)
                stretch = Player.CompositeArmStretchAmount.Quarter;
            else if (Projectile.frame == 1)
                stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
            else
                stretch = Player.CompositeArmStretchAmount.Full;
            owner.SetCompositeArmFront(true, stretch, Projectile.rotation - 1.57f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, owner.gfxOffY) - Main.screenPosition, frameBox, lightColor, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private void Shoot()
        {
            if (!owner.PickAmmo(owner.HeldItem, out int type, out float speed, out int damage, out float knockBack, out int ammoItemID, false))
            {
                Projectile.active = false;
                return;
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.rotation.ToRotationVector2() * (speed + 12), type, damage, knockBack, owner.whoAmI);
            proj.GetGlobalProjectile<GeodeBowGProj>().shotFromGeodeBow = true;
        }
    }

    internal class GeodeBowGrowth : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Projectile activator;

        public NPC target;

        public Vector2 offset;

        public float scaleFactor = 0;
        public float maxScale = 1;

        public float pulseCounter = 0f;

        private float[] crystalScales = new float[5];

        private Player owner => Main.player[Projectile.owner];

        public override void Load()
        {
            for (int i = 1; i <= 4; i++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "GeodeBowGrowthGore" + i.ToString());
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Growth");
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 500;
            Projectile.scale = 1;
            Projectile.hide = true;
            maxScale = Main.rand.NextFloat(0.85f, 1.15f);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override void AI()
        {
            if (activator is not null && !activator.active)
                activator = null;

            if (scaleFactor == 0)
            {
                for (int i = 0; i < 5; i++)
                    crystalScales[i] = Main.rand.NextFloat(0.6f, 1.4f);
            }

            Lighting.AddLight(Projectile.Center, Color.Magenta.ToVector3());
            pulseCounter += 0.05f;
            if (Projectile.timeLeft > 50)
            {
                if (scaleFactor < 1.2f)
                    scaleFactor += 0.03f;
            }
            else
                scaleFactor -= 0.03f;
            if (scaleFactor <= 0)
                Projectile.active = false;

            Projectile.scale = scaleFactor * maxScale;

            Projectile.rotation = offset.ToRotation() + 2.35f;
            if (!target.active)
            {
                Projectile.active = false;
                return;
            }
            Projectile.Center = target.Center + offset;

            Projectile proj = Main.projectile.Where(n => n.active && n != activator && n.Hitbox.Intersects(Projectile.Hitbox) && n.GetGlobalProjectile<GeodeBowGProj>().shotFromGeodeBow).FirstOrDefault();
            if (proj != null)
                Shatter(proj);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 1; i <= 5; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(Texture + "_Segment" + i.ToString()).Value;
                float progress = MathHelper.Clamp((scaleFactor * 5) - i, 0, 1);
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, progress * crystalScales[i - 1], SpriteEffects.None, 0f);
            }

            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Color color = Color.White * 0.8f * Projectile.scale;
            color.A = 0;
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, glowTex.Size() / 2, 1 + (0.1f * (float)Math.Sin(pulseCounter)), SpriteEffects.None, 0f);
            return false;
        }

        private void Shatter(Projectile proj)
        {
            proj.penetrate--;
            Helper.PlayPitched("Impacts/GlassExplodeShort", 1, Main.rand.NextFloat(0.1f, 0.3f), Projectile.Center);

            int counter = 0;

            foreach (Projectile otherCrystal in Main.projectile)
            {
                if (otherCrystal.active && otherCrystal.type == Projectile.type && (otherCrystal.ModProjectile as GeodeBowGrowth).target == target)
                    counter++;

                if (counter > 6)
                    break;
            }

            Core.Systems.CameraSystem.Shake += (int)Math.Min(11, 5 * (float)Math.Sqrt(counter));

            for (int k = 0; k < 12; k++)
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(30,30), ModContent.DustType<Dusts.ArtifactSparkles.GeodeArtifactSparkleFast>(), Main.rand.NextVector2Circular(7, 7), 0, default, Main.rand.NextFloat(0.85f, 1.15f));

            DistortionPointHandler.AddPoint(Projectile.Center, (float)Math.Sqrt(counter), 0.5f,
                    (intensity, ticksPassed) => intensity,
                    (progress, ticksPassed) => (float)Math.Sqrt(ticksPassed / 14f) + 0.5f,
                    (progress, intensity, ticksPassed) => ticksPassed <= 7);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GeodeBowExplosion>(), (int)(Math.Pow(counter, 0.7f) * Projectile.damage), Projectile.knockBack, owner.whoAmI, 0, (int)Math.Sqrt(counter) * 0.5f);

            for (int j = 0; j < 16; j++)
            {
                Vector2 direction = Main.rand.NextVector2CircularEdge(1, 1);
                Projectile shrapnel = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + (direction * 10), direction * Main.rand.Next(5, 10) * (float)Math.Sqrt(counter), ModContent.ProjectileType<GeodeBowShrapnel>(), 0, 0, owner.whoAmI);
                (shrapnel.ModProjectile as GeodeBowShrapnel).width = Main.rand.Next(30, 50);
                shrapnel.timeLeft = Main.rand.Next(90);
            }

            Vector2 vel = Vector2.Normalize(offset) * 5;
            for (int i = 1; i <= 4; i++)
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, vel.RotatedByRandom(0.4f), Mod.Find<ModGore>("GeodeBowGrowthGore" + i.ToString()).Type, Projectile.scale);
            Projectile.active = false;
        }
    }

    internal class GeodeBowExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public float radiusMult => Projectile.ai[1];

        public float Progress => 1 - (Projectile.timeLeft / 10f);

        private float Radius => (150 + (15 * Projectile.ai[0])) * (float)(Math.Sqrt(Progress)) * radiusMult;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Explosion");
        }

        public override void AI()
        {
            var crystals = Main.projectile.Where(x => x.active && x.type == ModContent.ProjectileType<GeodeBowGrowth>()).ToList();

            foreach (Projectile proj in crystals)
            {
                Vector2 line = proj.Center - Projectile.Center;
                line.Normalize();
                line *= Radius;

                if (!Collision.CheckAABBvLineCollision(proj.position, proj.Size, Projectile.Center, Projectile.Center + line))
                    continue;

                Vector2 vel = Vector2.Normalize((proj.ModProjectile as GeodeBowGrowth).offset) * 5;
                for (int i = 1; i <= 4; i++)
                    Gore.NewGore(Projectile.GetSource_FromThis(), proj.Center, vel.RotatedByRandom(0.4f), Mod.Find<ModGore>("GeodeBowGrowthGore" + i.ToString()).Type, proj.scale);
                proj.active = false;
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
                return true;

            return false;
        }
    }

    public class GeodeBowShrapnel : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.Invisible;

        public float width = 35;

        private List<Vector2> cache;

        private Trail trail;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Shrapnel");
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.94f;
            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 15; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 15)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => MathHelper.Lerp(1f, 0.35f, factor) * width * MathHelper.Lerp(0.2f, 1, Projectile.timeLeft / 90.0f), factor =>
            {
                return Color.Lerp(Color.Magenta, Color.White, Projectile.timeLeft / 90.0f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["alpha"].SetValue(1);
            BlendState oldState = Main.graphics.GraphicsDevice.BlendState;
            Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;

            trail?.Render(effect);

            Main.graphics.GraphicsDevice.BlendState = oldState;
        }
    }

    public class GeodeBowGProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool shotFromGeodeBow = false;

        public override void AI(Projectile projectile)
        {
            if (shotFromGeodeBow && Main.rand.NextBool(10))
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Dusts.ArtifactSparkles.GeodeArtifactSparkleFast>(), Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(0.85f, 1.15f) * projectile.scale);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (!shotFromGeodeBow)
                return;

            Projectile proj = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<GeodeBowGrowth>(), projectile.damage, projectile.knockBack, projectile.owner);
            var modProj = proj.ModProjectile as GeodeBowGrowth;

            Vector2 offset = (projectile.Center - target.Center);
            modProj.target = target;
            modProj.offset = offset + (Vector2.Normalize(offset) * 10);
            modProj.activator = projectile;
        }
    }
}