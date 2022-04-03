using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class GraveBlood : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = false;
            //dust.frame = new Rectangle(0, 0, 4, 4);
            dust.scale = 1.5f;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.15f;
            if (Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].HasTile && Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].collisionType == 1)
            {
                dust.alpha += 10;
                dust.velocity *= -0.1f;
            }

            dust.rotation = dust.velocity.ToRotation();
            dust.scale *= 0.99f;
            if (dust.scale < 1)
                dust.alpha += 10;
            if (dust.alpha > 240)
                dust.active = false;
            return false;
        }
    }
}