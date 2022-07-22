using StarlightRiver.Core;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class CogAmmoItem : ModItem
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cogshot");
            Tooltip.SetDefault("Bounces up to five times\nHalves damage on hit");
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 10;

            Item.value = Item.sellPrice(copper: 5);
            Item.rare = ItemRarityID.Green;

            Item.maxStack = 999;
            Item.damage = 10;
            Item.knockBack = 1f;

            Item.ammo = AmmoID.Bullet;
            Item.consumable = true;

            Item.DamageType = DamageClass.Ranged;
            Item.shoot = ModContent.ProjectileType<CogAmmoProjectile>();
            Item.shootSpeed = 4.5f;
        }
    }

    internal class CogAmmoProjectile : ModProjectile
    {
        internal int MaxBounces = 5;

        internal bool hasBounced;

        internal bool firstHit;

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cog");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 240;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f + Projectile.direction;

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] > 15)
                Projectile.velocity.Y += 0.96f;

            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //for some reason the BuzzSpark dust spawns super offset
            Vector2 dustPos = Projectile.Center + new Vector2(0f, 40f);
            if (!firstHit)
            {
                for (int k = 0; k < 8; k++)
                {
                    Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.BuzzSpark>(), Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(-0.5f, -0.2f), 0, new Color(255, 255, 60) * 0.8f, 1.3f);
                }
                Helper.PlayPitched("Impacts/Clink", 0.25f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);
                CameraSystem.Shake += 3;
                firstHit = true;
            }
            else
            {
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.BuzzSpark>(), Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(-0.4f, -0.1f), 0, new Color(255, 255, 60) * 0.8f, 1.1f);
                }
                Helper.PlayPitched("Impacts/Clink", 0.15f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);
            }

            Projectile.velocity.X *= -1f;

            Projectile.damage = Projectile.damage / 2;

            hasBounced = true;

            MaxBounces--;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Vector2 dustPos = Projectile.Center + new Vector2(0f, 40f);
            hasBounced = true;

            MaxBounces--;
            if (MaxBounces <= 0)
                Projectile.Kill();

            for (int k = 0; k < 6; k++)
            {
                Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.BuzzSpark>(), Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(-0.4f, -0.1f), 0, new Color(255, 255, 60) * 0.8f, 1.1f);
            }
            Helper.PlayPitched("Impacts/Clink", 0.10f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }
        //yeah yeah examplemod shhh
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale - (k / 10f), SpriteEffects.None, 0);
            }

            return true;
        }   
    }
}
