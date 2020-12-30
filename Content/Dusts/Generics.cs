using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class GenericFollow : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is Player)
                dust.position += (dust.customData as Player).velocity;

            dust.scale *= 0.95f;

            if (dust.scale < 0.1f) dust.active = false; return false;
        }
    }
}