using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Mushroom
{
	class JellyShroom : DummyTile
    {
        public override int DummyType => ProjectileType<JellyShroomDummy>();

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 7, 7, -1, SoundID.NPCDeath1, false, new Color(100, 200, 220), false, false, "Jelly Shroom");
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, 16 * 7, 16 * 7, ItemType<JellyShroomItem>(), 1);

            for (int k = 0; k < 35; k++)
                Dust.NewDust(new Vector2(i, j) * 16, 16 * 7, 16 * 7, DustType<Content.Dusts.BlueStamina>());
        }
    }

    class JellyShroomDummy : Dummy
    {
        public JellyShroomDummy() : base(TileType<JellyShroom>(), 7 * 16, 2 * 16) { }

        public override void Collision(Player Player)
        {
            if (Projectile.ai[1] == 0 && Player.velocity.Y > 0)
            {
                Projectile.ai[1] = 1;
                Player.velocity.Y *= -1;
                Player.velocity.Y -= 5;
                if (Player.velocity.Y > -10) Player.velocity.Y = -10;

                for (int k = 16; k < 96; k++)
                    Dust.NewDustPerfect(Projectile.position + new Vector2(k, Main.rand.Next(36)), DustType<Content.Dusts.BlueStamina>(), Vector2.One.RotatedByRandom(3.14f) * 2, 0, default, 0.9f);

                Terraria.Audio.SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/JellyBounce"), Player.Center);
            }
        }

        public override void Update()
        {
            for (int k = 16; k < 96; k++)
            {
                if (Main.rand.NextBool(120))
                {
                    float off = -2 * k * k / 357 + 232 * k / 357 - 1280 / 119;
                    Dust.NewDustPerfect(Projectile.position + new Vector2(k, 36 - off), DustType<Content.Dusts.BlueStamina>(), new Vector2(0, Main.rand.NextFloat(0.4f, 0.6f)), 0, default, 0.7f);
                }
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.4f, 0.7f));

            if (Projectile.ai[1] == 1)
                Projectile.ai[0]++;

            if (Projectile.ai[0] >= 90)
            {
                Projectile.ai[1] = 0;
                Projectile.ai[0] = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var back = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/JellyShroomBack").Value;
            var blob0 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom0").Value;
            var blob1 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom1").Value;
            var blob2 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom2").Value;
            var blob3 = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/JellyShroom3").Value;

            var pos = Projectile.position - Main.screenPosition;

            var mult = 0.05f;
            if (Projectile.ai[1] == 1)
                mult = 0.05f + 0.00533333f * Projectile.ai[0] - 0.0000592593f * Projectile.ai[0] * Projectile.ai[0];
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Draw(back, pos, lightColor);
            DrawBlob(spriteBatch, blob0, pos + new Vector2(12, 0), 0, mult);
            DrawBlob(spriteBatch, blob1, pos + new Vector2(52, 42), 1, 0.15f);
            DrawBlob(spriteBatch, blob2, pos + new Vector2(24, 40), 2, 0.15f);
            DrawBlob(spriteBatch, blob3, pos + new Vector2(16, 62), 3, 0.15f);

            return false;
        }

        private void DrawBlob(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, float offset, float mult)
        {
            float speed = Projectile.ai[1] == 1 ? 4 : 1;

            var sin = 1 + (float)Math.Sin(StarlightWorld.rottime * speed + offset) * mult;
            var sin2 = 1 + (float)Math.Sin(StarlightWorld.rottime * speed + offset + 1) * mult;
            var target = new Rectangle((int)pos.X + tex.Width / 2, (int)pos.Y + tex.Height / 2, (int)(tex.Width * sin), (int)(tex.Height * sin2));

            Color color = Projectile.ai[1] == 0 ? Color.White : Color.Lerp(new Color(255, 100, 100), Color.White, Projectile.ai[0] / 90f);
            spriteBatch.Draw(tex, target, null, color, 0, tex.Size() / 2, 0, 0);
        }
    }

    class JellyShroomItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Mushroom/JellyShroomItem";

        public JellyShroomItem() : base("Blue Jellyshroom", "Boing!", "JellyShroom", 0, null, false, Item.sellPrice(0, 1, 0, 0)) { }
    }
}
