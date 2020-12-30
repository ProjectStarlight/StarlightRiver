using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

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
            dust.noGravity = true;
            dust.noLight = false;
            //  dust.color = new Color(200, 90, 40);
            // dust.alpha = 25;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale *= 0.93f;
            dust.velocity *= 0.75f;
            dust.alpha += 15;
            if (dust.velocity.Length() <= 1)
                dust.active = false;
            float num64 = dust.scale * 1.4f;
            num64 *= 1.3f;
            if (num64 > 1f)
                num64 = 1f;
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num64, num64 * 0.45f, num64 * 0.2f);
            return false;
        }
    }
}