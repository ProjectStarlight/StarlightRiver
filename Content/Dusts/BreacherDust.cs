using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class BreacherDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = false;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale *= 0.98f;
            dust.velocity *= 0.98f;
            dust.alpha += 10;
            if (dust.alpha >= 255)
                dust.active = false;
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), dust.color.R * 0.01f, dust.color.G * 0.01f, dust.color.B * 0.01f);
            return false;
        }
    }
}