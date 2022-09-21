using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.Enums;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Dusts;

namespace StarlightRiver.Content.Items.Permafrost
{
    public class BookOfFrost : SmartAccessory
    {
        public BookOfFrost() : base ("Book Of Frost", "Melee critical strikes cause an icy explosion") {}
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void Load()
        {
            StarlightPlayer.OnHitNPCEvent += CritExplosion;
            StarlightPlayer.OnHitNPCWithProjEvent += ProjectileCritExplosion;
        }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1, silver: 25);
        }

        private void ProjectileCritExplosion(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (proj.CountsAsClass(DamageClass.Melee) && crit)
            {
                Helper.PlayPitched("Magic/FrostHit", 0.75f, Main.rand.NextFloat(-0.05f, 0.05f), target.Center);
                Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<FrostExplosion>(), (int)(damage * 1.25f), knockback * 0.25f, player.whoAmI);
            }
        }

        private void CritExplosion(Player player, Item Item, NPC target, int damage, float knockback, bool crit)
        {
            if (Item.CountsAsClass(DamageClass.Melee) && crit)
            {
                Helper.PlayPitched("Magic/FrostHit", 0.75f, Main.rand.NextFloat(-0.05f, 0.05f), target.Center);
                Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<FrostExplosion>(), (int)(damage * 1.25f), knockback * 0.25f, player.whoAmI);
            }
        }
    }

    class FrostExplosion : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        public float TimeFade => 1 - Projectile.timeLeft / 20f;
        public float Radius => EaseBuilder.EaseCubicOut.Ease(1 - (Projectile.timeLeft / 20f)) * 55f;

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
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

                if (Main.netMode != NetmodeID.Server)
                    Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot + Main.rand.NextFloat(1.1f, 1.3f)) * 2, 0,
                        Main.rand.NextBool() ? new Color(100, 150 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 255) : new Color(0, 165 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 225), 0.4f);

                for (int i = 0; i < 2; i++)
                {
                    var pos = Projectile.Center + Vector2.One.RotatedBy(rot) * Radius;

                    var vel = Vector2.One.RotatedBy(rot + Main.rand.NextFloat(1.1f, 1.3f)) * 2;
                    Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Gas>(), vel, 0, new Color(255, 255, 255) * 0.1f, Main.rand.NextFloat(4.5f, 6.5f));

                    vel = Vector2.One.RotatedBy(rot + Main.rand.NextFloat(1.1f, 1.3f)) * 2; //randomize velo for each dust
                    Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Gas>(), vel, 0, new Color(100, 150 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 255) * 0.5f, Main.rand.NextFloat(4.5f, 6.5f));

                    vel = Vector2.One.RotatedBy(rot + Main.rand.NextFloat(1.1f, 1.3f)) * 2;
                    Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Gas>(), vel, 0, new Color(0, 165 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 225) * 0.5f, Main.rand.NextFloat(4.5f, 6.5f));
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Helper.CheckCircularCollision(Projectile.Center, (int)Radius + 50, targetHitbox);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 240);
        }

        private void ManageCaches()
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 40; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            for (int k = 0; k < 40; k++)
            {
                cache[k] = (Projectile.Center + Vector2.One.RotatedBy(k / 19f * 6.28f) * (Radius));
            }

            while (cache.Count > 40)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(40 * 3.5f), factor => 25f, factor =>
            {
                return new Color(0, 165 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 225) * (float)Math.Sin(TimeFade * 3.14f) * 0.5f;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = cache[39];

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(40 * 3.5f), factor => 15f, factor =>
            {
                return new Color(100, 150 + (int)(100 * (float)Math.Sin(TimeFade * 3.14f)), 255) * (float)Math.Sin(TimeFade * 3.14f) * 0.5f;
            });

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = cache[39];
        }

        public void DrawPrimitives()
        {
            if (Projectile.ai[1] == 1f)
                return;

            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(5f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);
            trail2?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LightningTrail").Value);

            trail2?.Render(effect);
        }
    }
}
