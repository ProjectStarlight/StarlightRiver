using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class RainbowCrystal : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/RainbowCrystal";
            return true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            ConstructorInfo info = typeof(RainbowCrystal).GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, null, new Type[0], null);

            foreach (CustomAttributeData data in info.CustomAttributes)
            {
                Main.NewText(data.ToString());
            }
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 3, DustType<Dusts.Aurora>(), SoundID.Tink, false, Color.White, false, false, "Aurora Crystal");
            TileID.Sets.DrawsWalls[Type] = true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameX == 0 && tile.frameY == 0)
            {
                float sin2 = (float)Math.Sin(StarlightWorld.rottime + 0.2f * 0.2f);
                float cos = (float)Math.Cos(StarlightWorld.rottime + 0.2f);
                Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

                spriteBatch.Draw(Main.tileTexture[tile.type], (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, color);
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Permafrost/RainbowCrystalGlow"), (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, color * 2);
                Lighting.AddLight(new Vector2(i + 1, j + 1.5f) * 16, color.ToVector3() * 0.5f);
            }
        }

        class RainbowCrystalItem : QuickTileItem
        {
            public override string Texture => AssetDirectory.Debug;
            public RainbowCrystalItem() : base("RainbowCrystal Placer", "", TileType<RainbowCrystal>(), ItemRarityID.White) { }
        }
    }
}
