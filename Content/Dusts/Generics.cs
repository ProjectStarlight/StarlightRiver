using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Dusts
{
    public class GenericFollow : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is Player)
            {
                dust.position += (dust.customData as Player).velocity;
            }

            dust.scale *= 0.95f;

            if (dust.scale < 0.1f) { dust.active = false; }
            return false;
        }
    }
}