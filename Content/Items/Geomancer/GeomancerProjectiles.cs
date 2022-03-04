using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    public class EmeraldHeart : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Emerald Heart");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 5);
            player.HealEffect(5);
            player.statLife += healAmount;

            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 100);
    }

    public class RubyDagger : ModProjectile,IDrawAdditive
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

        Vector2 direction = Vector2.Zero;

        private List<float> oldRotation = new List<float>();

        private bool launched = false;
        private bool lockedOn = false;
        private int launchCounter = 15;
        private float rotation = 0f;

        private float radiansToSpin = 0f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ruby Dagger");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(16, 16);
            projectile.penetrate = 1;
            radiansToSpin = 6.28f * Main.rand.Next(2, 5) * Math.Sign(Main.rand.Next(-100,100));
            projectile.hide = true;

        }

        public override void AI()
        {

            oldRotation.Add(projectile.rotation);
            while (oldRotation.Count > projectile.oldPos.Length)
            {
                oldRotation.RemoveAt(0);
            }


            NPC target = Main.npc[(int)projectile.ai[0]];

            if (target.active && target.life > 0 && !lockedOn)
                direction = Vector2.Normalize(target.Center - projectile.Center);

            if (!launched)
            {
                projectile.timeLeft = 50;
                projectile.velocity *= 0.96f;
                rotation = MathHelper.Lerp(rotation, direction.ToRotation() + radiansToSpin, 0.02f);
                projectile.rotation = rotation + 1.57f; //using a variable here cause I think projectile.rotation automatically caps at 2 PI?

                float difference = Math.Abs(rotation - (direction.ToRotation() + radiansToSpin));
                if (difference < 0.7f || lockedOn)
                {
                    projectile.velocity *= 0.8f;
                    if (difference > 0.2f)
                    {
                        rotation += 0.1f * Math.Sign((direction.ToRotation() + radiansToSpin) - rotation);
                    }
                    else
                        rotation = direction.ToRotation() + radiansToSpin;
                    lockedOn = true;
                    launchCounter--;
                    projectile.Center -= direction * 3;
                    if (launchCounter <= 0)
                        launched = true;
                }
                else
                {
                    rotation += 0.15f * Math.Sign((direction.ToRotation() + radiansToSpin) - rotation);
                }
            }
            else
            {
                direction = Vector2.Normalize(target.Center - projectile.Center);
                projectile.velocity = direction * 30;
                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            Vector2 origin = new Vector2(tex.Width / 2, tex.Height);

            SpriteEffects effects = Math.Sign(radiansToSpin) == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            for (int k = projectile.oldPos.Length - 1; k > 0; k--) //TODO: Clean this shit up
            {
                Vector2 drawPos = projectile.oldPos[k] + (new Vector2(projectile.width, projectile.height) / 2);
                Color color = Color.White * (float)(((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length));
                if (k > 0 && k < oldRotation.Count)
                    spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, projectile.scale, effects, 0f);
            }

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, tex.Size() / 2, projectile.scale, effects, 0f);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
                Dust.NewDustPerfect(projectile.Center, DustID.RubyBolt, (projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(0.4f)), 0, default, 1.25f).noGravity = true; ;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
}