using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Abilities;
using Terraria.ModLoader;
using Terraria.ObjectData;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricOre : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override int DummyType => ProjectileType<VitricOreDummy>();
        public override void SetDefaults()
        {
            TileObjectData.newTile.DrawYOffset = 2;
            (this).QuickSetFurniture(2, 3, DustType<Dusts.Air>(), SoundID.Shatter, false, new Color(200, 255, 230), false, false, "Vitric Ore");
            minPick = int.MaxValue;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 12);
    }

    internal class VitricOreFloat : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override int DummyType => ProjectileType<VitricOreFloatDummy>();
        public override void SetDefaults()
        {
            (this).QuickSetFurniture(2, 2, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, new Color(200, 255, 230), false, false, "Vitric Ore");
            minPick = int.MaxValue;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 6);
    }

    internal class VitricOreDummy : Dummy
    {
        public VitricOreDummy() : base(TileType<VitricOre>(), 32, 48) { }

        public override void Collision(Player player)
        {
            if (AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                WorldGen.KillTile((int)projectile.position.X / 16, (int)projectile.position.Y / 16);

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
                }
            }
        }

        public override string Texture => AssetDirectory.VitricTile + "VitricOreGlow";

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(AssetDirectory.VitricTile + "VitricOreGlow");
            Color color = Helper.IndicatorColor;

            spriteBatch.Draw(tex, projectile.position - Vector2.One - Main.screenPosition, color);
        }
    }

    internal class VitricOreFloatDummy : Dummy
    {
        public VitricOreFloatDummy() : base(TileType<VitricOreFloat>(), 32, 32) { }

        public override void Collision(Player player)
        {
            if (AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                WorldGen.KillTile((int)projectile.position.X / 16, (int)projectile.position.Y / 16);

                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
                    Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
                }
            }
        }

        public override string Texture => AssetDirectory.VitricTile + "VitricOreFloatGlow";

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(AssetDirectory.VitricTile + "VitricOreFloatGlow");
            Color color = Helper.IndicatorColor;

            spriteBatch.Draw(tex, projectile.position - Vector2.One - Main.screenPosition, color);
        }
    }
}