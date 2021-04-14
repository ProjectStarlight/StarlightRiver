using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Temple
{
    class JarTall : DummyTile
    {
        public override int DummyType => ProjectileType<JarDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.UndergroundTempleTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 4, DustType<Content.Dusts.Stamina>(), SoundID.Shatter, false, new Color(204, 91, 50), false, false, "Stamina Jar");

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(1, 0.5f, 0.2f) * 0.3f);
            if (Main.rand.Next(4) == 0) Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, DustType<Content.Dusts.Stamina>(), new Vector2(0, -Main.rand.NextFloat()));
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/UndergroundTemple/JarTallGlow");
                Texture2D tex2 = GetTexture("StarlightRiver/Assets/Tiles/UndergroundTemple/JarTallGlow2");

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive);

                spriteBatch.End();
                spriteBatch.Begin();

                spriteBatch.Draw(tex, (Helper.TileAdj + new Vector2(i, j)) * 16 - Main.screenPosition, Color.White);
                spriteBatch.Draw(tex2, (Helper.TileAdj + new Vector2(i, j)) * 16 + new Vector2(-2, 0) - Main.screenPosition, Helper.IndicatorColor);

            }
        }
    }

    internal class JarDummy : Dummy, IDrawAdditive
    {
        public JarDummy() : base(TileType<JarTall>(), 32, 32) { }

        public override void Collision(Player player)
        {
            if (AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                WorldGen.KillTile(ParentX, ParentY);
                Main.PlaySound(SoundID.Shatter, projectile.Center);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + Vector2.UnitY * 16, tex.Frame(), Color.OrangeRed * 0.7f, 0, tex.Size() / 2, 0.8f, 0, 0);
        }
    }

    public class JarTallItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public JarTallItem() : base("Stamina Jar Placer (Tall)", "Places a stamina jar, for debugging.", TileType<JarTall>(), -12) { }
    }
}
