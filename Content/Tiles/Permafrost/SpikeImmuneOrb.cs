using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class SpikeImmuneOrb : DummyTile
    {
        public override int DummyType => ProjectileType<SpikeImmuneOrbDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/SpikeImmuneOrb";
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Aurora>(), SoundID.Tink, false, new Color(150, 255, 200));
            minPick = 9999;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            float time = Main.GameUpdateCount / 200f * 6.28f;

            if (tile.frameX == 0)
            {
                float off = i + j;
                float sin2 = (float)Math.Sin(time + off * 0.01f * 0.2f);
                float cos = (float)Math.Cos(time + off * 0.01f);

                drawColor = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

                Lighting.AddLight(new Vector2(i, j) * 16, drawColor.ToVector3() * 0.2f);
            }
            else
            {
                drawColor = Color.Black * 0;
            }

            spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One.RotatedBy(time) * 3f - Main.screenPosition, drawColor * 0.7f);
            spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One.RotatedBy(time + 1) * 3f - Main.screenPosition, drawColor * 0.6f);
            spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One.RotatedBy(time + 2) * 3f - Main.screenPosition, drawColor * 0.5f);
            spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One.RotatedBy(time + 3) * 3f - Main.screenPosition, drawColor * 0.3f);
            spriteBatch.Draw(Main.tileTexture[Type], (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One.RotatedBy(time + 4) * 3f - Main.screenPosition, drawColor * 0.25f);
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

                for (int k = 0; k < 40; k++)
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
