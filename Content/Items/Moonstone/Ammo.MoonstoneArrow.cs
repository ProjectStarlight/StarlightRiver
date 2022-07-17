using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class MoonstoneArrow : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonlit Arrow");
            Tooltip.SetDefault("Gains speed and power as it travels");
        }

        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.knockBack = 0.5f;
            Item.value = 10;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ProjectileType<MoonstoneArrowProj>();
            Item.shootSpeed = 5f;
            Item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(100);
            recipe.AddIngredient(ItemType<MoonstoneBarItem>(), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    internal class MoonstoneArrowProj : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.MoonstoneItem + "MoonstoneArrow";

        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private float trailWidth => 0.4f;

        private Vector2 velBase = Vector2.Zero;

        private float gravity = 0.15f;

        private int targetID = -1;

        private float charge = 0;

        private int damageIncrementer = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonlit Arrow");
        }
        public override void SetDefaults()
        {
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 400;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override bool ShouldUpdatePosition() => base.ShouldUpdatePosition();

        public override void AI()
        {
            if (velBase == Vector2.Zero)
                velBase = Projectile.velocity;
            velBase.Y += gravity * (0.5f + (charge * 1.7f));
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

            Lighting.AddLight(Projectile.Center, Color.BlueViolet.ToVector3() * 0.5f);

            Vector2 offset = Main.rand.NextVector2Circular(0.5f, 1f).RotatedBy(Projectile.rotation);
            if (charge < 1)
            {
                charge += 0.01f;

                damageIncrementer++;
                if (damageIncrementer % 10 == 0)
                    Projectile.damage++;
                Dust.NewDustPerfect((Projectile.Center - offset * 60) + (Projectile.velocity * 2), ModContent.DustType<MoonstoneArrowDust>(), offset * 2, 0, Color.BlueViolet, charge);
            }

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }

            Projectile.velocity = velBase * (0.5f + (charge * 1.7f));
        }

        private void ManageCaches()
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
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new RoundedTip(12), factor => (10 + factor * 25) * trailWidth, factor =>
            {
                return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X * charge;
            });

            trail.Positions = cache.ToArray();

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new RoundedTip(6), factor => (80 + 0 + factor * 0) * trailWidth, factor =>
            {
                return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f * charge;
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

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;

            Color glowColor = new Color(100, 20, 255, 0);
            for (float i = Projectile.rotation + (gravity * 6); i < Projectile.rotation + (gravity * 6) + 6.28f; i += 1.57f)
            {
                Vector2 offset = (i.ToRotationVector2() * charge * 13) + ((Projectile.rotation + 1.57f).ToRotationVector2() * 10);
                Main.spriteBatch.Draw(whiteTex, offset + Projectile.Center - Main.screenPosition, null, glowColor * charge * 0.25f, Projectile.rotation, whiteTex.Size() / 2, Projectile.scale * (float)Math.Sqrt(charge) * 2, SpriteEffects.None, 0f);
            }


            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            targetID = target.whoAmI;
        }
        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item68 with { Volume = (float)Math.Sqrt(charge), Pitch = Main.rand.NextFloat(0.8f,1.2f)}, Projectile.Center);
            Core.Systems.CameraSystem.Shake += (int)(6 * charge);

            for (int j = 0; j < 17; j++)
            {
                Vector2 direction = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust.NewDustPerfect((Projectile.Center + new Vector2(0, 40)) + (direction * 30), ModContent.DustType<MoonstoneArrowSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.Next(2, 10) * (float)Math.Sqrt(charge), 0, Color.Purple * 0.8f, 1.6f);
            }
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MoonstoneArrowRing>(), Projectile.damage, Projectile.knockBack, Projectile.owner, targetID, charge);
        }
    }

    internal class MoonstoneArrowRing : MoonfuryRing
    {
        protected override float Radius => Projectile.ai[1] * 120 * (float)Math.Sqrt(Math.Sqrt(Progress));
    }

    public class MoonstoneArrowDust : Glow
    {
        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= Vector2.One * 32 * dust.scale;
                dust.customData = dust.scale;
                dust.scale *= 0.1f;
            }

            bool ret = base.Update(dust);

            if (dust.customData is float)
            {
                dust.active = true;
                dust.scale *= (1 / 0.95f);
                dust.scale += (float)dust.customData * 0.1f;
                if (dust.scale >= (float)dust.customData)
                    dust.customData = true;
            }

            return ret;
        }
    }

    internal class MoonstoneArrowSpark : BuzzSpark
    {
        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(2.5f, 25).RotatedBy(dust.rotation) * dust.scale;
                dust.customData = 1;
            }

            dust.frame.Y++;
            dust.frame.Height--;

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;

            dust.color.A -= 5;

            dust.velocity.X *= 0.98f;
            dust.velocity.Y *= 0.95f;

            dust.velocity.Y += 0.15f;

            float mult = 1;


            dust.shader.UseSecondaryColor(new Color((int)(255 * (1 - (dust.fadeIn / 30f))), 0, 0) * mult);
            dust.shader.UseColor(dust.color * mult);
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.02f);

            if (dust.fadeIn > 40)
                dust.active = false;
            return false;
        }
    }
}