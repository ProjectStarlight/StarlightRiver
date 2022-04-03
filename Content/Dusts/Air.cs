using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class Air : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 1.4f;
            dust.color.R = 160;
            dust.color.G = 235;
            dust.color.B = 255;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * (1 - dust.fadeIn);
        }

        public override bool Update(Dust dust)
        {
            dust.position.Y += dust.velocity.Y * 2;
            dust.velocity.Y += 0.01f;
            dust.position.X += dust.velocity.X * 2;
            dust.rotation += 0.06f;

            dust.scale *= 0.97f;
            dust.color *= 0.995f;

            if (dust.scale < 0.4f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class AirLegacyWindsAnimation : ModDust
    {
        private int timer = 0;

        public override string Texture => AssetDirectory.Dust + "Air";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            timer = 30;
            dust.color.R = 170;
            dust.color.G = 235;
            dust.color.B = 255;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            if (dust.customData is Player Player)
            {
                return dust.color * (1 - Vector2.Distance(dust.position, Player.Center) / 50f);
            }
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is Player Player)
            {
                dust.rotation = Vector2.Distance(dust.position, Player.Center) * 0.1f;
                dust.position += dust.velocity;

                dust.velocity = Vector2.Normalize(dust.position - Player.Center) * -4;
                dust.scale *= 0.95f;
                timer--;
                if (timer == 0 || Vector2.Distance(dust.position, Player.Center) < 1)
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

    public class AirGravity : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "Air";

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 1.4f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position.Y += dust.velocity.Y * 2;
            dust.velocity.Y += 0.01f;
            dust.position.X += dust.velocity.X * 2;
            dust.rotation += 0.05f;

            dust.scale *= 0.97f;

            if (dust.scale < 0.4f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class AirSetColorNoGravity : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "Air";
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.8f;
            dust.color.R = 170;
            dust.color.G = 235;
            dust.color.B = 255;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += 0.05f;
            dust.color *= 0.99f;

            dust.scale *= 0.98f;

            if (dust.scale < 0.4f)
            {
                dust.active = false;
            }
            return false;
        }
    }

    public class AirDash : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "Air";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.position -= Vector2.One * 32;

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.fadeIn <= 0 ? new Color(120, 255, 255) * (dust.alpha / 255f) : Color.Transparent;
        }

        public override bool Update(Dust dust)
        {
            dust.scale = (2f - Math.Abs(dust.fadeIn) / 30f) * 0.15f;
            Vector2 currentCenter = dust.position + Vector2.One * 32 * dust.scale;
            dust.fadeIn -= 3;
            dust.scale = (2f - Math.Abs(dust.fadeIn) / 30f) * 0.15f;
            Vector2 nextCenter = dust.position + Vector2.One * 32 * dust.scale;

            dust.position += currentCenter - nextCenter;

            dust.alpha = 150 - (int)(Math.Abs(dust.fadeIn) / 60f * 150);
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;

            dust.color = dust.fadeIn <= 0 ? new Color(100, 220, 255) * (dust.alpha / 255f) : Color.Transparent;
            dust.shader.UseColor(dust.color);

            if (dust.fadeIn <= -60) dust.active = false;
            return false;
        }
    }

    public class Void : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "Air";

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.1f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 3f;
            dust.color.R = 130;
            dust.color.G = 20;
            dust.color.B = 235;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.05f;

            dust.scale *= 0.94f;

            if (dust.scale < 0.2f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}