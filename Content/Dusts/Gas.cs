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

    public class NoxiousGas : Gas 
    {
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.rotation = Main.rand.NextFloat(6.28f);
            dust.frame = new Rectangle(0, 0, 34, 36);
        }
       
        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            dust.position += dust.velocity;
            dust.scale *= 1.02f;
            dust.velocity *= 0.92f;
            dust.rotation += 0.01f;
            dust.color = new Color(20, 255, 200) * (float)System.Math.Sin(dust.fadeIn / 60f * 3.14f) * 0.02f;

            if (dust.fadeIn >= 60) dust.active = false;
            return false;
        }
    }
}