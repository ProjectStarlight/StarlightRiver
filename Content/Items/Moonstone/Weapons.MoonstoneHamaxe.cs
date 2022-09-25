using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Buffs;
using System.Linq;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;

namespace StarlightRiver.Content.Items.Moonstone
{
    public class MoonstoneHamaxe : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.altFunctionUse == 2;
        public override bool AltFunctionUse(Player player) => true;
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Press <right> to charge up a slam that destroys walls");
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 32;

            Item.damage = 15;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 15;

            Item.hammer = 60;
            Item.axe = 25;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;

            Item.value = Item.sellPrice(silver: 35);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shootSpeed = 1f;
            Item.useTurn = true;
        }

        public override void UseItemHitbox(Player Player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (Main.rand.NextBool(15))
            {
                Dust.NewDustPerfect(hitbox.TopLeft() + new Vector2(Main.rand.NextFloat(hitbox.Width), Main.rand.NextFloat(hitbox.Height)),
                ModContent.DustType<Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.3f, 1.2f) * Player.direction, -Main.rand.NextFloat(0.3f, 0.5f)), 0,
                new Color(Main.rand.NextFloat(0.15f, 0.30f), Main.rand.NextFloat(0.2f, 0.30f), Main.rand.NextFloat(0.3f, 0.5f), 0f), Main.rand.NextFloat(0.15f, 0.40f));
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.noMelee = true;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noUseGraphic = true;
                Item.shoot = ModContent.ProjectileType<MoonstoneHamaxeHoldout>();
                Item.UseSound = null;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonstoneHamaxeHoldout>()] > 0)
                    return false;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noUseGraphic = false;
                Item.shoot = 0;
                Item.noMelee = false;
                Item.UseSound = SoundID.Item1;
            }

            return base.CanUseItem(player);
        }
    }

    class MoonstoneHamaxeHoldout : ModProjectile
    {
        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private const int MAXCHARGE = 45;

        private const int MAXSWINGTIME = 25;

        private Vector2 direction;

        private int flashTimer;

        private bool initialized;

        private bool swung;

        private bool slammed;

        private ref float Charge => ref Projectile.ai[0];

        private ref float swingTimer => ref Projectile.ai[1];

        private Player owner => Main.player[Projectile.owner];

        public override bool? CanDamage() => swung;
        public override string Texture => AssetDirectory.MoonstoneItem + "MoonstoneHamaxe";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Hamaxe");
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 36;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                direction = Projectile.velocity;
                direction.Normalize();
                Projectile.rotation = Utils.ToRotation(direction);
                Projectile.netUpdate = true;
            }

            if (flashTimer > 0)
                flashTimer--;

            Projectile.Center = owner.MountedCenter + Projectile.rotation.ToRotationVector2() * 30f;
            owner.heldProj = Projectile.whoAmI;
            owner.ChangeDir(Projectile.direction);
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            if (Main.myPlayer != owner.whoAmI)
                return;

            if (Main.mouseRight && !swung)
            {
                if (Charge < MAXCHARGE)
                {
                    if (Charge == (MAXCHARGE - 1))
                    {
                        //DustHelper.DrawStar(owner.Center + Projectile.rotation.ToRotationVector2() * 40f, ModContent.DustType<Dusts.GlowFastDecelerate>(), pointAmount: 5, mainSize: 1.5f, dustDensity: 0.7f, pointDepthMult: 0.3f, rotationAmount: Projectile.rotation, dustSize: 0.5f, color: new Color(120, 120, 255));
                        DustHelper.DrawDustImage(owner.Center + Projectile.rotation.ToRotationVector2() * 40f, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0.05f,
                            ModContent.Request<Texture2D>(Texture + "_Crescent").Value, 0.7f, 0, new Color(120, 120, 255));
                        SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                        flashTimer = 15;
                    }
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(0, -3f * Projectile.direction, EaseBuilder.EaseCubicInOut.Ease(Charge / MAXCHARGE));
                    Charge++;
                }
                Projectile.timeLeft = 2;
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(Projectile.direction == -1 ? 80f : 100f));
            }
            else
            {
                swung = true;
                if (swingTimer < MAXSWINGTIME)
                {
                    Projectile.timeLeft = 2;
                    Projectile.rotation = (Projectile.velocity.ToRotation() + MathHelper.Lerp(MathHelper.Lerp(0, -3f * Projectile.direction, Charge / MAXCHARGE), 2f * Projectile.direction, EaseBuilder.EaseCubicInOut.Ease(swingTimer / MAXSWINGTIME)));
                    swingTimer++;
                }

                if (swingTimer == 10)
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);

                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(Projectile.direction == -1 ? 80f : 100f));

                Point tilePos = Projectile.Bottom.ToTileCoordinates();
                if (swingTimer > 10 && WorldGen.SolidTile(tilePos.X, tilePos.Y) && !slammed)
                {
                    slammed = true;
                    Projectile.timeLeft = 30;
                    swingTimer = MAXSWINGTIME;
                    DoSlam();
                }

                if (slammed)
                    owner.velocity *= 0.5f;

                Dust.NewDustPerfect((owner.Center + Projectile.rotation.ToRotationVector2() * 40f) + Main.rand.NextVector2Circular(10f, 10f), ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(120, 120, 255), 0.35f);
            }

            if (Main.netMode != NetmodeID.Server && swingTimer > 1)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override void Kill(int timeLeft)
        {
            Item Item = owner.HeldItem;

            Item.noUseGraphic = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = 0;
            Item.noMelee = false; //hacky solution but its the only one i could find for wonky stuff happening to the left click after right clicking
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitives();
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            SpriteEffects flip = owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0;
            float rotation = Projectile.rotation + MathHelper.PiOver4 + (owner.direction == -1 ? MathHelper.PiOver2 : 0f);
            Vector2 drawPos = (owner.MountedCenter + new Vector2(0f, owner.gfxOffY)) + Projectile.rotation.ToRotationVector2() * 35f - Main.screenPosition + Main.rand.NextVector2Circular(0.5f, 0.5f);

            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, rotation, tex.Size() / 2f, Projectile.scale, flip, 0f);

            if (flashTimer > 0)
                Main.spriteBatch.Draw(glowTex, drawPos, null, Color.Lerp(new Color(120, 120, 255, 0), Color.Transparent, 1f - (flashTimer / 15f)), rotation, glowTex.Size() / 2f, Projectile.scale, flip, 0f);

            if (swingTimer > 0)
                Main.spriteBatch.Draw(glowTex, drawPos, null, Color.Lerp(Color.Transparent, new Color(120, 120, 255, 0), (swingTimer * 0.5f) / (float)MAXSWINGTIME), rotation, glowTex.Size() / 2f, Projectile.scale, flip, 0f);

            return false;
        }

        public void DoSlam()
        {
            if (Main.myPlayer == owner.whoAmI)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Bottom, Vector2.Zero, ModContent.ProjectileType<MoonstoneHamaxeRing>(), 0, 0, owner.whoAmI, MathHelper.Lerp(20, 80, Charge / MAXCHARGE));

            Collision.HitTiles(Projectile.Bottom + Main.rand.NextVector2Circular(15f, 15f), Vector2.UnitY * Main.rand.NextFloat(2f, 4f), 16, 16);
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with {Volume = 1.35f, PitchVariance = 0.1f }, Projectile.Center);
            Core.Systems.CameraSystem.Shake += 12;

            for (int i = 0; i < 55; i++)
            {
                double rad = (i / 55f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad) * 0.5f, (float)Math.Cos(rad));
                offset *= 4;
                Dust.NewDustPerfect(Projectile.Bottom, ModContent.DustType<Dusts.GlowFastDecelerate>(), offset.RotatedBy(Projectile.rotation + MathHelper.PiOver4 * Projectile.direction), 0, new Color(120, 120, 255), 0.85f);
            }

            for (int i = 0; i < 25; i++)
            {
                Dust.NewDustPerfect(Projectile.Bottom, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(-7.5f, 7.5f), 0, new Color(120, 120, 255), 0.65f);
                Dust.NewDustPerfect(Projectile.Bottom, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY.RotatedByRandom(0.4f) * -Main.rand.NextFloat(6f), 0, new Color(120, 120, 255), 0.65f);
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 35; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 35)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 35, new TriangularTip(40 * 4), factor => factor * 18f, factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                if (Projectile.timeLeft <= 30 && slammed)
                    return new Color(120, 20 + (int)(100 * factor.X), 255) * MathHelper.Lerp(1f, 0f, 1f - Projectile.timeLeft / 30f) * factor.X;

                return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 35, new TriangularTip(40 * 4), factor => factor * 10f, factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                if (Projectile.timeLeft <= 30 && slammed)
                    return new Color(120, 20 + (int)(100 * factor.X), 255) * MathHelper.Lerp(0.85f, 0f, 1f - Projectile.timeLeft / 30f) * factor.X;

                return new Color(120, 20 + (int)(60 * factor.X), 255) * factor.X;
            });

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            if (!swung)
                return;

            Main.spriteBatch.End();
            Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(-Projectile.timeLeft * 0.05f);
            effect.Parameters["repeats"].SetValue(8f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);

            trail2?.Render(effect);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    internal class MoonstoneHamaxeRing : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        protected float Progress => 1 - (Projectile.timeLeft / 25f);

        protected virtual float Radius => Projectile.ai[0] * EaseBuilder.EaseCircularInOut.Ease(Progress);

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
            for (int k = 0; k < 8; k++)
            {
                float rot = Main.rand.NextFloat(0, 6.28f);

                Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * (Radius * 0.75f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.One.RotatedBy(rot), 0, new Color(120, 120, 255), Main.rand.NextFloat(0.35f, 0.4f));
            }

            for (int i = 0; i < 40; i++)
            {
                double rad = (i / 39f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= Main.rand.NextFloat(Radius);
                Vector2 pos = Projectile.Center + offset;
                Point point = pos.ToTileCoordinates();
                WorldGen.KillWall(point.X, point.Y);
            }
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;

            for (int i = 0; i < 40; i++)
            {
                double rad = (i / 39f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(Projectile.Center + offset);
            }

            while (cache.Count > 40)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 100 * (1 - Progress), factor =>
            {
                return new Color(120, 120, 255);
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 35 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 40f / 39f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + offset;
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

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
}
