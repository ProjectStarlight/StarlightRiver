using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    public class GauntletSpawner : ModProjectile, IGauntletNPC
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.ai[1]++;

            Vector2 cinderPos = Projectile.Top + Main.rand.NextVector2Circular(95, 95);
            Dust cinder = Dust.NewDustPerfect(cinderPos, ModContent.DustType<Dusts.Cinder>(), -Vector2.UnitY.RotatedBy(cinderPos.AngleTo(Projectile.Center)) * Main.rand.NextFloat(-2, 2), 0, Bosses.GlassMiniboss.Glassweaver.GlowDustOrange, 1f);
            cinder.customData = Projectile.Center + Main.rand.NextVector2Circular(40, 40);

            if (Projectile.ai[1] > 70)
            {
                NPC.NewNPC(Entity.GetSource_Misc("SLR:GlassGauntlet"), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)Projectile.ai[0]);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //do animation here
            return false;
        }
    }
}
