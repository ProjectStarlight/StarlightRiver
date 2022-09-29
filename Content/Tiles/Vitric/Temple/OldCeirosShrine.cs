﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class OldCeirosShrine : DummyTile
	{
        public override int DummyType => ProjectileType<OldCeirosShrineDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);
            Main.tileLighted[Type] = true;

            this.QuickSetFurniture(4, 3, DustID.Stone, SoundID.Tink, false, new Color(217, 193, 154));
        }

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
            (r, g, b) = (0.8f, 0.6f, 0.4f);
		}

		public override bool SpawnConditions(int i, int j)
		{
            var tile = Framing.GetTileSafely(i, j);
            return tile.TileFrameX < 16 && tile.TileFrameY == 0;
		}

		public override bool RightClick(int i, int j)
        {
            if (Main.LocalPlayer.HeldItem.type == ItemType<Items.DebugStick>())
            {
                var tile = Framing.GetTileSafely(i, j);

                if (tile.TileFrameX < 16 && tile.TileFrameY == 0)
                {
                    tile.TileFrameX++;
                    tile.TileFrameX %= 4;
                }

                return true;
            }

            return false;
        }

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
            var tile = Framing.GetTileSafely(i, j);

            if(tile.TileFrameX < 16 && tile.TileFrameY == 0)
			{
                var tileTex = TextureAssets.Tile[tile.TileType].Value;
                spriteBatch.Draw(tileTex, (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(0, 0, 16, 16), Lighting.GetColor(i, j));

                return false;
			}

            return true;
		}
	}

    class OldCeirosShrineDummy : Dummy, IDrawAdditive
	{
        public OldCeirosShrineDummy() : base(TileType<OldCeirosShrine>(), 16, 16) { }

		public override void Update()
		{
            if (Main.rand.NextBool(20))
            {
                var pos = Projectile.position + Vector2.UnitX * Main.rand.NextFloat(64);
                Dust.NewDustPerfect(pos, DustType<Dusts.Aurora>(), Vector2.UnitY * Main.rand.NextFloat(-4, -1), 0, new Color(255, Main.rand.Next(150, 255), 50), Main.rand.NextFloat(0.5f, 1f));
            }
		}

		public override void PostDraw(Color lightColor)
		{
            var tex = Request<Texture2D>(AssetDirectory.VitricTile + "OldCeirosOrnament" + Parent.TileFrameX).Value;
            var sin = (float)Math.Sin(Main.GameUpdateCount / 30f);
            var pos = Projectile.position - Main.screenPosition + new Vector2(32, -32 + sin * 4);

            Main.spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            var texGlow = Request<Texture2D>(AssetDirectory.VitricBoss + "LongGlow").Value;
            var pos = Projectile.position - Main.screenPosition + new Vector2(33, 10);

            var sin = (float)Math.Sin(Main.GameUpdateCount / 18f);

            spriteBatch.Draw(texGlow, pos + Vector2.UnitY * 2, null, new Color(255, 180, 100) * (0.5f + sin * 0.1f), 0, new Vector2(texGlow.Width / 2, texGlow.Height), 0.32f, 0, 0);
            spriteBatch.Draw(texGlow, pos, null, new Color(255, 255, 100) * (0.8f + sin * 0.2f), 0, new Vector2(texGlow.Width / 2, texGlow.Height), 0.12f, 0, 0);
        }
	}

    class OldCeirosShrineItem : QuickTileItem 
    {
        public OldCeirosShrineItem() : base("Old Ceiros Shrine", "Debug Item", "OldCeirosShrine", 0) { }
    }
}
