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
            Color newColor = dust.color * Utils.GetLerpValue(800, 10, dust.fadeIn, true);
            newColor.A = 128;
            dust.color = newColor;
            return dust.color;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= 0.38f;
            dust.frame = new Rectangle(0, 0, 160, 160); 
            dust.fadeIn = 0;
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
            dust.noLightEmittence = false;
        }

        public override bool Update(Dust dust)
        {
            if (!dust.noLightEmittence)
            {
                dust.position -= Vector2.One * 80 * dust.scale;
                dust.noLightEmittence = true;
                dust.velocity.Y *= 1.1f;
            }
            if (dust.customData is Vector2 target)
            {
                Vector2 pos = dust.position + Vector2.One * 80 * dust.scale;
                dust.velocity = Vector2.Lerp(dust.velocity, pos.DirectionTo(target) * pos.Distance(target) * 0.1f, 0.03f);
                if (pos.Distance(target) < 3f)
                    dust.active = false;
            }

            dust.shader.UseColor(dust.color * Utils.GetLerpValue(0, 4, dust.fadeIn, true));

            dust.fadeIn++;
            if (dust.fadeIn > 100)
                dust.active = false;

            if (dust.noGravity)
            {
                if (Main.rand.NextBool())
                    dust.velocity += Main.rand.NextVector2Circular(0.2f, 0.05f);
                dust.velocity.Y -= 0.01f;

                if (dust.fadeIn > 20)
                    dust.velocity *= 0.98f;
            }
            else if (dust.fadeIn > 10)
            {
                dust.velocity.Y += 0.25f;
                if (Collision.SolidTiles(dust.position + new Vector2(0, 22), 8, 8))
                    dust.velocity = Vector2.Zero;
            }

            dust.velocity *= 0.98f;

            dust.position += dust.velocity;

            return false;
        }
    }
}