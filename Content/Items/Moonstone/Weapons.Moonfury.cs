using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Buffs;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Moonstone
{
    public class Moonfury : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        private int cooldown = 0;

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Melee;
            Item.width = 36;
            Item.height = 38;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7.5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.shootSpeed = 14f;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<MoonfuryProj>();
            Item.useTurn = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonfury");
            Tooltip.SetDefault("Call down a shard of moonstone, afflicting enemies with Dreamfire\nAfflicted enemies take extra damage on hit from Moonfury");
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 8);
            recipe.AddIngredient(ItemID.Starfury, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

        public override void HoldItem(Player Player)
        {
            cooldown--;
            base.HoldItem(Player);
        }

        public override void UseItemHitbox(Player Player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if(Main.rand.Next(15) == 0)
            {
                Dust.NewDustPerfect(hitbox.TopLeft() + new Vector2(Main.rand.NextFloat(hitbox.Width), Main.rand.NextFloat(hitbox.Height)),
                ModContent.DustType<Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.3f, 1.2f) * Player.direction, -Main.rand.NextFloat(0.3f, 0.5f)), 0,
                new Color(Main.rand.NextFloat(0.15f, 0.30f), Main.rand.NextFloat(0.2f, 0.30f), Main.rand.NextFloat(0.3f, 0.5f), 0f), Main.rand.NextFloat(0.15f, 0.40f));
            }
        }

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
            Vector2 direction = new Vector2(0, -1);
            direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            position = Main.MouseWorld + (direction * 800);

            direction *= -10;
            velocity = direction;
            damage *= 2;
        }

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (cooldown > 0)
                return false;

            cooldown = 75;
            return true;
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (target.HasBuff(ModContent.BuffType<MoonfuryDebuff>()))
            {
                Core.Systems.CameraSystem.Shake += 10;
                int index = target.FindBuffIndex(ModContent.BuffType<MoonfuryDebuff>());
                target.DelBuff(index);
                Helper.PlayPitched("Magic/Shadow1", 1, Main.rand.NextFloat(-0.1f, 0.1f));
                damage += 5;
                damage += (int)(target.defense / 5f);
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Zero, ModContent.ProjectileType<MoonfuryRing>(), 0, 0, player.whoAmI);

                for (int i = 0; i < 16; i++)
                    Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(Main.rand.NextFloat(6.28f)) * Main.rand.NextFloat(12), 0, new Color(50, 50, 255), 0.4f);
            }
        }
    }
    internal class MoonfuryProj : ModProjectile, IDrawPrimitive, IDrawAdditive
    {
        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private float trailWidth = 1;
        private bool stuck = false;

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 50;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 6;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonfury Spike");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
                Projectile.ai[0] = Main.MouseWorld.Y;

            if (Projectile.Bottom.Y > Projectile.ai[0])
                Projectile.tileCollide = true;
            else
                Projectile.tileCollide = false;

            if (!stuck)
            {
                var d = Dust.NewDustPerfect(Projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 0.8f);
                d.customData = Main.rand.NextFloat(0.6f, 1.3f);
                d.fadeIn = 10;
                Dust.NewDustPerfect(Projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f).fadeIn = 10;
                ManageCaches();

                Projectile.rotation = Projectile.velocity.ToRotation() - 1.44f;
            }
            else
            {
                Projectile.friendly = false;

                if (Projectile.timeLeft <= 30)
                    Projectile.alpha += 10;

                trailWidth *= 0.93f;

                if (trailWidth > 0.05f)
                    trailWidth -= 0.05f;
                else
                    trailWidth = 0;
            }
            ManageTrail();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck)
            {
                //add hit sound effect here
                for (int k = 0; k <= 8; k++)
                {
                    Vector2 pos = Projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15);
                    Vector2 velocity = (-Vector2.UnitY).RotatedByRandom(0.7f) * Main.rand.NextFloat(1f, 2f);
                    Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), velocity, 0, new Color(50, 50, 255), Main.rand.NextFloat(0.4f, 0.8f)).fadeIn = 10;

                    if (Main.rand.Next(3) == 0)
                    {
                        Dust.NewDustPerfect(Projectile.TopLeft + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
                        ModContent.DustType<Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.4f)), 0,
                        new Color(Main.rand.NextFloat(0.25f, 0.30f), Main.rand.NextFloat(0.25f, 0.30f), Main.rand.NextFloat(0.35f, 0.45f), 0f), Main.rand.NextFloat(0.2f, 0.4f));
                    }
                }

                Core.Systems.CameraSystem.Shake += 10;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Bottom, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), 0, 0, Projectile.owner).timeLeft = 194;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item96, Projectile.Center);
                stuck = true;
                Projectile.extraUpdates = 0;
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = 90;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            var pos = (Projectile.Bottom + new Vector2(0, 20)) - Main.screenPosition;
            Main.spriteBatch.Draw(tex, pos, null, lightColor * (1 - (Projectile.alpha / 255f)), Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<MoonfuryDebuff>(), 150);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 50; i++)
                {
                    cache.Add(Projectile.Bottom + new Vector2(0, 20));
                }
            }

            if (Projectile.oldPos[0] != Vector2.Zero)
                cache.Add(Projectile.oldPos[0] + new Vector2(Projectile.width / 2, Projectile.height) + new Vector2(0, 20));

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new RoundedTip(12), factor => (10 + factor * 25) * trailWidth, factor =>
            {
                return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X * trailWidth;
            });

            trail.Positions = cache.ToArray();

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 50, new RoundedTip(6), factor => (80 + 0 + factor * 0) * trailWidth, factor =>
            {
                return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f * trailWidth;
            });

            trail2.Positions = cache.ToArray();

            if (Projectile.velocity.Length() > 1)
            {
                trail.NextPosition = Projectile.Bottom + new Vector2(0, 20) + Projectile.velocity;
                trail2.NextPosition = Projectile.Bottom + new Vector2(0, 20) + Projectile.velocity;
            }
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["repeats"].SetValue(8f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture2"].SetValue(Terraria.GameContent.TextureAssets.MagicPixel.Value);

            trail2?.Render(effect);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture + "_Additive").Value;
            Color color = Color.White * (1 - (Projectile.alpha / 255f));
            spriteBatch.Draw(tex, (Projectile.Bottom + new Vector2(0, 20)) - Main.screenPosition, null, color * 0.5f, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), Projectile.scale, SpriteEffects.None, 0);
        }
    }

    internal class MoonfuryRing : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        protected float Progress => 1 - (Projectile.timeLeft / 10f);

        protected virtual float Radius => 66 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override string Texture => AssetDirectory.MoonstoneItem + "MoonfuryProj";

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
            DisplayName.SetDefault("Orbital Strike");
        }

        public override void AI()
        {
            ManageCaches();
            ManageTrail();
        }

        public override bool PreDraw(ref Color lightColor)
		{
            return false;
		}

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == (int)Projectile.ai[0])
                return false;

            return base.CanHitNPC(target);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
                return true;

            return false;
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
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
            {
                return new Color(100, 0, 255);
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
            {
                return Color.Lerp(new Color(180, 180, 255), new Color(85, 85, 200), Progress);
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

    class MoonfuryDebuff : SmartBuff
    {
        public override string Texture => AssetDirectory.Debug;

        public MoonfuryDebuff() : base("Dreamfire", "Next Moonfury hit has increased damage", false) { }

        public override void Update(NPC NPC, ref int buffIndex)
        {
            Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, new Color(50, 50, 255), 0.4f).velocity = Vector2.Zero;

            var d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.Aurora>(), 0, 0, 0, new Color(20, 20, 100), 0.8f);
            d.customData = Main.rand.NextFloat(0.6f, 1.3f);
            d.fadeIn = 10; 
        }
    }
}