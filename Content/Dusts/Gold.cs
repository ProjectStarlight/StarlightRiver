using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class GoldNoMovement : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "Gold";
            return base.Autoload(ref name, ref texture);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 3f;
            dust.color.R = 255;
            dust.color.G = 220;
            dust.color.B = 100;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += 0.05f;

            dust.scale *= 0.97f;
            if (dust.scale < 0.2f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class GoldWithMovement : GoldNoMovement
    {
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;

            dust.scale *= 0.92f;
            if (dust.scale < 0.3f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class GoldPlayerRotation : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "Gold";
            return base.Autoload(ref name, ref texture);
        }

        private int timer = 0;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            timer = 30;
            dust.scale *= 2;
            dust.color.R = 255;
            dust.color.G = 220;
            dust.color.B = 100;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            Player player = Main.LocalPlayer;
            dust.rotation = Vector2.Distance(dust.position, player.Center) * 0.1f;

            if (dust.customData is int) { dust.customData = (int)dust.customData - 1; }
            dust.position += dust.velocity;

            float rot = dust.velocity.ToRotation();

            if (dust.customData != null && (int)dust.customData <= 0)
            {
                rot += (float)(Math.PI * 2) / (20 * 18);
                if (rot >= (float)Math.PI * 2)
                {
                    rot = 0;
                }

                Vector2 offset = (player.Center - dust.position);
                dust.position.X = player.Center.X + (float)Math.Cos(rot) * offset.Length();
                dust.position.Y = player.Center.Y + (float)Math.Sin(rot) * offset.Length();

                dust.velocity = Vector2.Normalize(dust.position - player.Center) * -3.9f;

                dust.scale *= 0.97f;
                timer--;

                if (timer == 0 || dust.scale <= 0.31f)
                {
                    dust.active = false;
                }
            }
            else
            {
                dust.velocity *= 0.95f;
            }
            return false;
        }
    }

    public class GoldSlowFade : GoldNoMovement
    {
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.fadeIn++;

            dust.scale *= 0.999f;
            dust.alpha = 155 + (int)(dust.fadeIn > 300 ? (dust.fadeIn - 300) / 300 * 100 : (300 - dust.fadeIn) / 300 * 100);

            if (dust.fadeIn > 600) dust.active = false;

            return false;
        }
    }
}
