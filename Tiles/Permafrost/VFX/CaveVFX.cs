using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Projectiles.Dummies;
using StarlightRiver.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Tiles.Permafrost.VFX
{
    class CaveVFX : DummyTile
    {
        public override int DummyType => ProjectileType<CaveVFXDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSet(this, 200, 0, SoundID.Tink, new Color(255, 255, 255), 0);
    }

    class CaveVFXDummy : Dummy
    {
        public CaveVFXDummy() : base(TileType<CaveVFX>(), 16, 16) { }

        public override void Update()
        {
            projectile.ai[0]++;

            if (projectile.ai[0] % 10 == 0)
            {
                int diff = (int)projectile.ai[0] % 200;
                int off = 2 * diff - (int)Math.Pow(diff, 2) / 100;

                for (int k = 0; k < off; k++)
                {
                    float x = projectile.ai[0] % 400;
                    float y = projectile.ai[0] % 200;
                    Dust.NewDustPerfect(projectile.Center + new Vector2(x, y) + Vector2.One.RotatedByRandom(0.1f) * (k), DustType<Dusts.Star>());
                }
            }
        }
    }
}
