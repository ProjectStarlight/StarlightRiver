using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class Cinder : ModDust
    {
        public override string Texture => AssetDirectory.Keys + "GlowHarsh";

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color newColor = dust.color * (float)Math.Pow(Utils.GetLerpValue(180f, 30f, dust.fadeIn, true), 1.5f);
            newColor.A = 0;
            dust.color = newColor;
            return dust.color;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= 0.4f;
            dust.frame = new Rectangle(0, 0, 160, 160); 
            dust.fadeIn = 0;
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
            {
                dust.position -= Vector2.One * 80 * dust.scale;
                dust.customData = true;
            }

            dust.shader.UseColor(dust.color);

            dust.fadeIn++;
            if (dust.fadeIn > 180)
                dust.active = false;

            if (dust.noGravity)
            {
                if (Main.rand.NextBool())
                    dust.velocity += Main.rand.NextVector2Circular(0.2f, 0.05f);
                dust.velocity.Y -= 0.01f;
            }
            else
                dust.velocity.Y += 0.1f;

            dust.velocity *= 0.96f;

            dust.position += dust.velocity;

            return false;
        }
    }
}