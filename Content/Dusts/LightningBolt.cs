using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    class LightningBolt : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * MathHelper.Min(1, dust.fadeIn / 20f) * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 43, 74);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(21.5f, 37) * dust.scale;
                dust.customData = Main.rand.NextFloat(6.28f);

                dust.rotation = dust.velocity.ToRotation() + 1.57f;

                if (Main.rand.NextBool())
                    dust.rotation += 3.14f;
            }
      
            dust.position += dust.velocity;

            dust.velocity *= 0.955f;
            dust.color *= 0.96f;

            dust.shader.UseColor(dust.color * MathHelper.Min(1, dust.fadeIn / 20f) * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.5f + (float)dust.customData) * 0.5f));
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 60)
                dust.active = false;
            return false;
        }
    }
}
