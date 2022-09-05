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
using Terraria.Audio;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Breacher
{
    public class ScrappodItem : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap Pod");
            Tooltip.SetDefault("Shatters into scrapnel after reaching the mouse cursor\nIs only able to shatter after a short period of time");
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 10;

            Item.value = Item.sellPrice(copper: 10);
            Item.rare = ItemRarityID.Orange;

            Item.maxStack = 999;
            Item.damage = 8;
            Item.knockBack = 1.5f;

            Item.ammo = AmmoID.Bullet;
            Item.consumable = true;

            Item.DamageType = DamageClass.Ranged;
            Item.shoot = ModContent.ProjectileType<ScrappodProjectile>();
            Item.shootSpeed = 3.5f;
        }
    }
    
    internal class ScrappodProjectile : ModProjectile
    {
        private bool HasTouchedMouse;

        private bool initialized = false;

        private float distanceToExplode = 130;
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrappod");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;
            distanceToExplode = Main.rand.Next(145, 175);

            Projectile.aiStyle = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                if (Projectile.Distance(Main.MouseWorld) > distanceToExplode)
                    distanceToExplode = Projectile.Distance(Main.MouseWorld) * Main.rand.NextFloat(0.9f,1.1f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.velocity *= 1.025f;

            if (distanceToExplode < 0)
                HasTouchedMouse = true;

            if (Main.myPlayer == Projectile.owner && Projectile.timeLeft < 230 && HasTouchedMouse) //only explodes into scrap after a certain amount of time to prevent "shotgunning"
                ExplodeIntoScrap();

            distanceToExplode -= Projectile.velocity.Length();
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                //for some reason the BuzzSpark dust spawns super offset 
                Dust.NewDustPerfect(Projectile.Center + new Vector2(0f, 28f), ModContent.DustType<Dusts.BuzzSpark>(), (Projectile.velocity * 0.75f).RotatedByRandom(MathHelper.ToRadians(10f)), 0, new Color(255, 255, 60) * 0.8f, 1.15f);

                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), (Projectile.velocity * Main.rand.NextFloat(0.5f, 0.6f)).RotatedByRandom(MathHelper.ToRadians(15f)), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.25f, 0.5f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);

            Vector2 origin = sourceRectangle.Size() / 2f;

            float offsetX = -10f;
            origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            Color drawColor = Projectile.GetAlpha(lightColor);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture,
                Projectile.position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public void ExplodeIntoScrap()
        {
            for (int i = 0; i < 3; i++)
            {              
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f))) * Main.rand.NextFloat(0.8f, 1.1f);
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<ScrappodScrapnel>(), (int)(Projectile.damage * 0.66f), 1f, Projectile.owner);
                }                  
            }
            //maybe better sound here
            Helper.PlayPitched("Guns/Scrapshot", 0.2f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);
            Projectile.Kill();
        }
    }

    internal class ScrappodScrapnel : ModProjectile
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrapnel");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;

            Projectile.frame = Main.rand.Next(3);
        }

        public override void AI()
        {
            Projectile.rotation += 0.25f * Projectile.direction;
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] > 20)
                Projectile.velocity.Y += 0.94f;

            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f))) * Main.rand.NextFloat(-0.25f, -0.35f);

                Dust.NewDustPerfect(Projectile.Center + new Vector2(0f, 28f), ModContent.DustType<Dusts.BuzzSpark>(), vel, 0, new Color(255, 255, 60) * 0.8f, 0.95f);

                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), vel * 1.2f, 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.4f));
            }
            SoundEngine.PlaySound(SoundID.NPCHit4.WithPitchOffset(Main.rand.NextFloat(-0.1f, 0.1f)).WithVolumeScale(0.5f), Projectile.position);
        }
    }
}