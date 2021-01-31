using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassSlash : ModProjectile
    {
        public NPC Parent => Main.npc[(int)projectile.ai[0]];

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 200;
            projectile.height = 80;
            projectile.hostile = true;
            projectile.timeLeft = 20;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 20)
                Main.PlaySound(
                    StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/GlassMinibossSword").SoundId,
                    -1, -1,
                    StarlightRiver.Instance.GetLegacySoundSlot(SoundType.Custom, "Sounds/GlassMinibossSword").Style,
                    1.1f, 0.2f
                    );

            if (Parent != null) projectile.Center = Parent.Center + Vector2.UnitX * (projectile.ai[1] == -1 ? 120 : -120);
        }
    }
}
