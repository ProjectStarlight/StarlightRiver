using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	class SpringBubble : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 20, 20);
            dust.fadeIn = 10;
            dust.velocity.X = Main.rand.NextFloat(6.28f);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White * (dust.alpha / 240f) * (1 - dust.fadeIn / 10f);
        }

        public override bool Update(Dust dust)
        {
            if(dust.fadeIn > 0)
                dust.fadeIn--;

            dust.velocity.X += 0.08f;
            dust.position.Y += dust.velocity.Y;
            dust.position.X += (float)Math.Sin(dust.velocity.X) * 0.3f;
            dust.alpha -= 1;

            if (dust.alpha <= 0)
                dust.active = false;

            Tile tile = Framing.GetTileSafely((int)dust.position.X / 16, (int)dust.position.Y / 16);
            if (tile .LiquidAmount == 0)
                dust.alpha = 0;

            return false;
        }
    }
}
