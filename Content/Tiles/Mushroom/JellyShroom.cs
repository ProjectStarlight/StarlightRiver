using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Mushroom
{
    class JellyShroom : DummyTile
    {
        public override int DummyType => ProjectileType<JellyShroomDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 7, 7, DustType<Content.Dusts.BlueStamina>(), 1, false, new Microsoft.Xna.Framework.Color(100, 200, 220), false, false, "Jelly Shroom");
        }
    }

    class JellyShroomDummy : Dummy
    {
        public JellyShroomDummy() : base(TileType<JellyShroom>(), 7 * 16, 2 * 16) { }

        public override void Collision(Player player)
        {
            if (projectile.ai[1] == 0 && player.velocity.Y > 0)
            {
                projectile.ai[1] = 1;
                player.velocity.Y *= -1;
                player.velocity.Y -= 5;
                if (player.velocity.Y > -10) player.velocity.Y = -10;

                for (int k = 16; k < 96; k++)
                    Dust.NewDustPerfect(projectile.position + new Vector2(k, Main.rand.Next(36)), DustType<Content.Dusts.BlueStamina>(), Vector2.One.RotatedByRandom(3.14f) * 2, 0, default, 0.9f);

                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/JellyBounce"), player.Center);
            }
        }

        public override void Update()
        {
            for (int k = 16; k < 96; k++)
            {
                if (Main.rand.Next(120) == 0)
                {
                    float off = -2 * k * k / 357 + 232 * k / 357 - 1280 / 119;
                    Dust.NewDustPerfect(projectile.position + new Vector2(k, 36 - off), DustType<Content.Dusts.BlueStamina>(), new Vector2(0, Main.rand.NextFloat(0.4f, 0.6f)), 0, default, 0.7f);
                }
            }

            Lighting.AddLight(projectile.Center, new Vector3(0.2f, 0.4f, 0.7f));

            if (projectile.ai[1] == 1)
                projectile.ai[0]++;

            if (projectile.ai[0] >= 90)
            {
                projectile.ai[1] = 0;
                projectile.ai[0] = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var back = GetTexture("StarlightRiver/Assets/Tiles/Mushroom/JellyShroomBack");
            var blob0 = GetTexture("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom0");
            var blob1 = GetTexture("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom1");
            var blob2 = GetTexture("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom2");
            var blob3 = GetTexture("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom3");

            var pos = projectile.position - Main.screenPosition;

            var mult = 0.05f;
            if (projectile.ai[1] == 1)
                mult = 0.05f + 0.00533333f * projectile.ai[0] - 0.0000592593f * projectile.ai[0] * projectile.ai[0];

            spriteBatch.Draw(back, pos, lightColor);
            DrawBlob(spriteBatch, blob0, pos + new Vector2(12, 0), 0, mult);
            DrawBlob(spriteBatch, blob1, pos + new Vector2(52, 42), 1, 0.15f);
            DrawBlob(spriteBatch, blob2, pos + new Vector2(24, 40), 2, 0.15f);
            DrawBlob(spriteBatch, blob3, pos + new Vector2(16, 62), 3, 0.15f);

            return false;
        }

        private void DrawBlob(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, float offset, float mult)
        {
            float speed = projectile.ai[1] == 1 ? 4 : 1;

            var sin = 1 + (float)Math.Sin(StarlightWorld.rottime * speed + offset) * mult;
            var sin2 = 1 + (float)Math.Sin(StarlightWorld.rottime * speed + offset + 1) * mult;
            var target = new Rectangle((int)pos.X + tex.Width / 2, (int)pos.Y + tex.Height / 2, (int)(tex.Width * sin), (int)(tex.Height * sin2));

            Color color = projectile.ai[1] == 0 ? Color.White : Color.Lerp(new Color(255, 100, 100), Color.White, projectile.ai[0] / 90f);
            spriteBatch.Draw(tex, target, null, color, 0, tex.Size() / 2, 0, 0);
        }
    }

    class JellyShroomItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public JellyShroomItem() : base("Blue Jellyshroom", "Boing!", TileType<JellyShroom>(), 0) { }
    }
}
