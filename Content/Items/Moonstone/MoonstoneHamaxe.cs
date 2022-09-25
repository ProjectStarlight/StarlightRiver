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

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.noMelee = true;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noUseGraphic = true;
                Item.shoot = ModContent.ProjectileType<MoonstoneHamaxeHoldout>();
                if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonstoneHamaxeHoldout>()] > 0)
                    return false;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noUseGraphic = false;
                Item.shoot = 0;
                Item.noMelee = false;
            }

            return base.CanUseItem(player);
        }
    }

    class MoonstoneHamaxeHoldout : ModProjectile
    {
        private const int MAXCHARGE = 45;

        private const int MAXSWINGTIME = 25;

        private Vector2 direction;

        private bool initialized;

        private bool swung;

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

            Projectile.Center = owner.Center + Projectile.rotation.ToRotationVector2() * 30f;
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
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(0, -2f * Projectile.direction, EaseBuilder.EaseCubicInOut.Ease(Charge / MAXCHARGE));
                    Charge++;
                }

                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            }
            else
            {
                swung = true;
                if (swingTimer < MAXSWINGTIME)
                {
                    Projectile.rotation = (Projectile.velocity.ToRotation() + MathHelper.Lerp(MathHelper.Lerp(0, -2f * Projectile.direction, Charge / MAXCHARGE), 1.5f * Projectile.direction, EaseBuilder.EaseCubicInOut.Ease(swingTimer / MAXSWINGTIME)));
                    swingTimer++;
                }
                else
                    Projectile.Kill();

                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));

                Point tilePos = Projectile.Bottom.ToTileCoordinates();
                if (swingTimer > 10 && WorldGen.SolidTile(tilePos.X, tilePos.Y))
                {
                    Projectile.Kill();
                    DoSlam();
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Item Item = owner.HeldItem;

            Item.noUseGraphic = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = 0;
            Item.noMelee = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects flip = owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0;
            float rotation = Projectile.rotation + MathHelper.PiOver4 + (owner.direction == -1 ? MathHelper.PiOver2 : 0f);
            Vector2 drawPos = owner.Center + Projectile.rotation.ToRotationVector2() * 35f - Main.screenPosition + Main.rand.NextVector2Circular(0.5f, 0.5f);

            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, rotation, tex.Size() / 2f, Projectile.scale, flip, 0f);
            return false;
        }

        public void DoSlam()
        {
            if (Main.myPlayer == owner.whoAmI)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Bottom, Vector2.Zero, ModContent.ProjectileType<MoonstoneHamaxeRing>(), 0, 0, owner.whoAmI, MathHelper.Lerp(20, 80, Charge / MAXCHARGE));

            Collision.HitTiles(Projectile.Bottom, Vector2.UnitY * -Main.rand.NextFloat(2f, 4f), 32, 32);
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
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
            ManageCaches();
            ManageTrail();

            
            for (int i = 0; i < 33; i++)
            {
                double rad = (i / 32f) * 6.28f;
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

            for (int i = 0; i < 33; i++)
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(Projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 45 * (1 - Progress), factor =>
            {
                return new Color(100, 0, 255);
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 25 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 33f / 32f;
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
