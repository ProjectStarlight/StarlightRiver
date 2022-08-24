using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    public class MushroomDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.color = Color.White;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.98f;
            dust.rotation += 0.12f;
            dust.scale *= 0.97f;
            if (!dust.noGravity)
                dust.velocity.Y += 0.15f;
            if (dust.scale <= 0.2f)
                dust.active = false;
            return false;
        }
    }
}