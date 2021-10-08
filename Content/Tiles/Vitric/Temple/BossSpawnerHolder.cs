using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class BossSpawnerHolder : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            (this).QuickSetFurniture(3, 2, DustType<Dusts.Stone>(), SoundID.Tink, false, new Color(0, 255, 255), false, false, "Mysterious Relic");
        }

        public override bool NewRightClick(int i, int j)
        {
            WorldGen.KillTile(i, j);
            Item.NewItem(Main.LocalPlayer.Center, ItemType<Items.Vitric.GlassIdolPremiumEdition>());

            return true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameX == 18 && tile.frameY == 0)
            {
                Texture2D tex = GetTexture(AssetDirectory.VitricItem + "GlassIdolPremiumEdition");
                spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(0, -32 + (float)System.Math.Sin(Main.GameUpdateCount * 0.02f) * 8), tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);
            }
        }
    }

    class BossSpawnerHolderItem : QuickTileItem
    {
        public BossSpawnerHolderItem() : base("Boss Spawner Holder", "", TileType<BossSpawnerHolder>(), 1, AssetDirectory.VitricTile) { }
    }
}
