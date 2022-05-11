using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class Gas : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "GasChaos";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.rotation = Main.rand.NextFloat(6.28f);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor) => dust.color;

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            dust.position += dust.velocity;
            dust.scale *= 0.98f;
            dust.velocity *= 0.97f;
            dust.rotation += 0.15f;
            dust.color = Color.White * (float)System.Math.Sin(dust.fadeIn / 30f * 3.14f) * 0.05f;

            if (dust.fadeIn >= 30) dust.active = false;
            return false;
        }
    }

    public class GasGreen : Gas { }
}