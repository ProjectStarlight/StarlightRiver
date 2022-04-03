using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.OvergrowBoss.OvergrowBossProjectile
{
	internal class Phase1Bolt : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 900;
            Projectile.extraUpdates = 3;
            Projectile.hostile = true;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f), DustType<Content.Dusts.GoldWithMovement>(),
                Vector2.Normalize(Projectile.velocity.RotatedBy(1.58f)) * (float)Math.Sin(StarlightWorld.rottime * 16) * 0.6f, 0, default, 0.8f);
        }

        public override void Kill(int timeLeft)
        {
            //Terraria.Audio.SoundEngine.PlaySound(ModLoader.GetMod("StarlightRiver").GetLegacySoundSlot(SoundType.Custom, "Sounds/ProjectileImpact1").WithVolume(0.5f), Projectile.Center);
            for (int k = 0; k < 20; k++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
            }
        }
    }
}