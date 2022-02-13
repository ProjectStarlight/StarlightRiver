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
	class Mirror : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            (this).QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 255, 255), false, true, "Mirror");
        }

        public override bool NewRightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            //0 = topleft, 1 = topright, 2 = bottomright, 3 = bottomleft
            if (tile.frameX < 3) tile.frameX++; //x frame is rotation
            else tile.frameX = 0;

            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StoneSlide"), new Vector2(i, j) * 16);

            return true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D tex = GetTexture(AssetDirectory.VitricTile + "MirrorOver");

            spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 - Main.screenPosition, tex.Frame(), Lighting.GetColor(i, j), tile.frameX * 1.57f, tex.Size() / 2 + Vector2.One * 4, 1, 0, 0);
        }
    }
}
