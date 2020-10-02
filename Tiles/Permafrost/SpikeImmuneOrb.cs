using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Projectiles.Dummies;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Permafrost
{
    class SpikeImmuneOrb : DummyTile
    {
        public override int DummyType => ProjectileType<SpikeImmuneOrbDummy>();

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Aurora>(), SoundID.Tink, false, new Color(150, 255, 200));
            minPick = 9999;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.frameX == 0)
            {
                float off = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;
                float sin2 = (float)Math.Sin(StarlightWorld.rottime + off * 0.01f * 0.2f);
                float cos = (float)Math.Cos(StarlightWorld.rottime + off * 0.01f);

                drawColor = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

                if(Main.rand.Next(2) == 0)
                    Dust.NewDust(new Vector2(i, j) * 16, 16, 16, DustType<Dusts.Aurora>(), 0, 0, 0, drawColor);

                Lighting.AddLight(new Vector2(i, j) * 16, drawColor.ToVector3() * 0.4f);
            }
            else
            {
                drawColor = Color.Black * 0;
            }
        }
    }

    class SpikeImmuneOrbDummy : Dummy
    {
        public SpikeImmuneOrbDummy() : base(TileType<SpikeImmuneOrb>(), 16, 16) { }

        public override void Collision(Player player)
        {
            if (Parent.frameX == 0)
            {
                player.AddBuff(BuffType<Buffs.SpikeImmuneBuff>(), 90);
                Main.PlaySound(SoundID.DD2_DarkMageAttack.SoundId, (int)player.Center.X, (int)player.Center.Y, SoundID.DD2_DarkMageHealImpact.Style, 0.5f, 1);

                for(int k = 0; k < 40; k++)
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Starlight>(), Vector2.One.RotatedByRandom(3.14f) * Main.rand.NextFloat(25), 0, Color.White, 0.5f);

                Parent.frameX = 16;
            }
        }

        public override void Update()
        {
            if (!Main.dayTime && Main.time == 0)
                Parent.frameX = 0;
        }
    }
}
