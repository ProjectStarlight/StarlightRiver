using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    class GlowLine : ModDust
    {
        public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            if (dust.fadeIn <= 2)
                return Color.Transparent;

            return dust.color * MathHelper.Min(1, dust.fadeIn / 20f);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 8, 128);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(4, 64) * dust.scale;
                dust.customData = 1;
            }

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;

            dust.velocity *= 0.98f;
            dust.color *= 0.97f;

            if (dust.fadeIn <= 2)
                dust.shader.UseColor(Color.Transparent);
            else
                dust.shader.UseColor(dust.color);

            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 60)
                dust.active = false;
            return false;
        }
    }

    class GlowLineFast : GlowLine
	{
		public override bool Update(Dust dust)
		{
            dust.color *= 0.95f;
            dust.fadeIn += 3;
            return base.Update(dust);
		}
	}
}
