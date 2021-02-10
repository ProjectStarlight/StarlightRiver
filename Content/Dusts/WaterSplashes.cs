using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public abstract class QuickSplash : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override void SetDefaults()
        {
            updateType = 33;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 170;
            dust.velocity *= 0.5f;
            dust.velocity.Y += 1f;
        }
    }

    public sealed class CorruptJungleSplash : QuickSplash { }

    public sealed class BloodyJungleSplash : QuickSplash { }

    public sealed class HolyJungleSplash : QuickSplash { }
}