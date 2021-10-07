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
    class TutorialText : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Debug;
            return true;
        }

        public override void SetDefaults()
        {
            (this).QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 0, 0), false, true, "");
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var pos = (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 - Main.screenPosition;

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Welcome to our demo blah blah blah this is filler test blah blah filler text this is to make sure whatever egshels writes will fit blah blah blah blah blah blah", 250, Main.fontItemStack, 0.8f), pos, Color.White, 0.8f);

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Melee", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-650, 150), Color.White, 1f);
            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Ranged", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-500, 150), Color.White, 1f);
            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Magic", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-350, 150), Color.White, 1f);
            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Consumables", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-210, 150), Color.White, 1f);

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("The forbidden winds ability shall make you cum big globs of the goopy white stuff haha filler text filler text filler text filler text filler text filler text", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-550, -360), Color.White, 0.8f);
        }
    }

    class TutorialTextItem : QuickTileItem
    {
        public TutorialTextItem() : base("Tutorial Text", "Well this is awkward.", TileType<TutorialText>(), 1, AssetDirectory.VitricTile) { }
    }
}
