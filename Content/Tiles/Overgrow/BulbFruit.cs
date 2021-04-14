using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class BulbFruit : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override int DummyType => ProjectileType<BulbFruitDummy>();

        public override bool SpawnConditions(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameY == 0 && (tile.frameX == 0 || tile.frameX == 34)) return true;
            else return false;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Dusts.GoldNoMovement>(), SoundID.Grass, false, new Color(255, 255, 200));

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameY == 0 && tile.frameX == 0) { r = 0.8f; g = 0.7f; b = 0.4f; }
            else { r = 0; g = 0; b = 0; }
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameY == 0 && tile.frameX == 34)
            {
                if (Main.rand.Next(8) == 0)
                {
                    for (int x = i; x <= i + 1; x++)
                        for (int y = j; y <= j + 1; y++)
                            Main.tile[x, y].frameX -= 34;
                }
            }
        }

        // TODO scalie impl
        public override void KillMultiTile(int i, int j, int frameX, int frameY) { }
    }

    internal class BulbFruitDummy : Dummy
    {
        public BulbFruitDummy() : base(TileType<BulbFruit>(), 32, 32) { }

        public override void Collision(Player player)
        {
            Tile tile = Main.tile[ParentX - 1, ParentY - 1];
            if (tile.frameX == 0 && tile.frameY == 0 && AbilityHelper.CheckWisp(player, projectile.Hitbox))
            {
                for (int k = 0; k < 40; k++) Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.2f, 1.4f));
                tile.frameX = 34;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Tile tile = Main.tile[ParentX - 1, ParentY - 1];

            Texture2D tex2 = GetTexture(AssetDirectory.OvergrowTile + "BulbFruit"); //Draws the bulb itself
            Rectangle frame = new Rectangle((tile.frameX == 0 && tile.frameY == 0) ? 0 : 32, 0, 32, 32);
            float offset = (float)Math.Sin(StarlightWorld.rottime) * 3;

            spriteBatch.Draw(tex2, projectile.Center + new Vector2(offset, 0) - Main.screenPosition, frame, Lighting.GetColor(ParentX, ParentY), 0, Vector2.One * 16, 1, 0, 0);

            if (tile.frameX == 0 && tile.frameY == 0) //Draws the glowing indicator
            {
                Texture2D tex = GetTexture(AssetDirectory.OvergrowTile + "BulbFruitGlow");

                spriteBatch.Draw(tex, projectile.Center + new Vector2(offset, 6) - Main.screenPosition, tex.Frame(), Helper.IndicatorColor, 0, tex.Size() / 2, 1, 0, 0);
                Dust.NewDust(projectile.position, 32, 32, DustType<Dusts.GoldNoMovement>(), 0, 0, 0, default, 0.3f);
                Lighting.AddLight(projectile.Center, new Vector3(1, 0.8f, 0.4f));
            }

            for (int k = 2; k <= 30; k++) //Draws the vine
            {
                if (Main.tile[ParentX, ParentY - k].active()) break;
                Texture2D tex = GetTexture(AssetDirectory.OvergrowTile + "VineOvergrowFlow");
                float sway = (float)Math.Sin(StarlightWorld.rottime + k * 0.2f) * 3;

                spriteBatch.Draw(tex, projectile.Center + new Vector2(sway - 8, k * -16) - Main.screenPosition, new Rectangle(16 * k % 3, 0, 16, 16), Lighting.GetColor(ParentX, ParentY - k));

                if (Main.rand.Next(5) == 0 && tile.frameX == 0 && tile.frameY == 0) Dust.NewDust(projectile.Center - new Vector2(10, k * 16 - 8), 16, 16, DustType<Content.Dusts.GoldWithMovement>(), 0, -3, 0, default, 0.3f);
            }
        }
    }

    internal class BulbFruitItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;
        public BulbFruitItem() : base("Bulb Fruit", "", TileType<BulbFruit>(), 1) { }
    }

}