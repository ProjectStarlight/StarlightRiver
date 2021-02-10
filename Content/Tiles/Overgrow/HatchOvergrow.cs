using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class HatchOvergrow : DummyTile
    {
        public override int DummyType => ProjectileType<HatchDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 1, DustType<Dusts.GoldSlowFade>(), SoundID.Tink, false, new Color(255, 255, 220));
        }
    }

    internal class HatchDummy : Dummy, IDrawAdditive
    {
        public HatchDummy() : base(TileType<HatchOvergrow>(), 32, 16) { }

        public override void Update()
        {
            if (Main.rand.Next(8) == 0)
            {
                float rot = Main.rand.NextFloat(-0.5f, 0.5f);
                Dust.NewDustPerfect(projectile.Center + new Vector2(0, 10).RotatedBy(rot), 244, new Vector2(0, 1).RotatedBy(rot), 0, default, 0.7f);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Vector2 pos = projectile.Center - Main.screenPosition;
            Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Overgrow/Shine");
            Color col = new Color(160, 160, 120);

            for (int k = 0; k <= 5; k++)
            {
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.5f, (float)Math.Sin(StarlightWorld.rottime + k) * 0.5f, new Vector2(8, 0), 1, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.3f, (float)Math.Sin(StarlightWorld.rottime + k + 0.5f) * 0.5f, new Vector2(8, 0), 1.4f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.6f, (float)Math.Sin(StarlightWorld.rottime + k + 0.7f) * 0.3f, new Vector2(8, 0), 1.2f, 0, 0);
            }
        }
    }

    internal class BigHatchOvergrow : DummyTile
    {
        public override int DummyType => ProjectileType<BigHatchDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = false;
            minPick = 210;
            AddMapEntry(new Color(255, 255, 220));
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            Lighting.AddLight(new Vector2(i - 8, j + 4) * 16, new Vector3(0.6f, 0.6f, 0.5f) * 2);
        }
    }

    internal class BigHatchDummy : Dummy, IDrawAdditive
    {
        public BigHatchDummy() : base(TileType<BigHatchOvergrow>(), 16, 16) { }

        public override void Update()
        {
            projectile.ai[0] += 0.01f;

            if (projectile.ai[0] >= 6.28f)
                projectile.ai[0] = 0;

            if (Main.rand.Next(5) == 0)
            {
                float rot = Main.rand.NextFloat(-0.7f, 0.7f);
                Dust.NewDustPerfect(projectile.Center + new Vector2(24, -24), DustType<Dusts.GoldSlowFade>(), new Vector2(0, 0.4f).RotatedBy(rot + 0.7f), 0, default, 0.4f - Math.Abs(rot) / 0.7f * 0.2f);
            }
        }
        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Vector2 pos = projectile.Center + new Vector2(24, -32) - Main.screenPosition;
            Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Overgrow/Shine");
            Color col = new Color(160, 160, 120);

            for (int k = 0; k <= 5; k++)
            {
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.4f, (float)Math.Sin(projectile.ai[0] + k) * 0.5f + 0.7f, new Vector2(8, 0), 2.6f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.3f, (float)Math.Sin(projectile.ai[0] + k + 0.5f) * 0.6f + 0.7f, new Vector2(8, 0), 4f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), col * 0.5f, (float)Math.Sin(projectile.ai[0] + k + 0.9f) * 0.3f + 0.7f, new Vector2(8, 0), 3.2f, 0, 0);
            }
        }
    }
}