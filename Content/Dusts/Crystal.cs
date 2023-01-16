using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	class Crystal : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, Main.rand.Next(2) * 20, 20, 20);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.96f;
            dust.scale *= 0.98f;
            dust.rotation += dust.fadeIn;

            if (dust.scale <= 0.2f) dust.active = false;
            return false;
        }
    }
}
