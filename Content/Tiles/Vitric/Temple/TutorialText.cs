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
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            (this).QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 0, 0), false, true, "");
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var pos = (new Vector2(i + 0.5f, j + 0.5f) + Helper.TileAdj) * 16 - Main.screenPosition;

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Welcome to Starlight River! This is a demo version of Starlight River with a focus on Ceiros, the third (and currently most complete) boss in the mod. Try out some of the gear available at this progression point and tell us what you think!", 250, Main.fontItemStack, 0.8f), pos, Color.White, 0.8f);

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Here is some of the gear from the mod for you to play with, be sure to check the tooltips and set bonuses!", 500, Main.fontItemStack, 0.8f), pos + new Vector2(-650, 50), Color.White, 0.8f);

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Melee", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-650, 150), Color.White, 1f);
            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Ranged", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-500, 150), Color.White, 1f);
            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Magic", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-350, 150), Color.White, 1f);
            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Consumables", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-210, 150), Color.White, 1f);

            Utils.DrawBorderString(spriteBatch, Helper.WrapString("Forbidden Winds is the first obtainable ability, granting a dash that allows you to maneuver with ease and interact with objects. Like all abilities, it's regularly used throughout progression at all stages of the game past obtainment.", 300, Main.fontItemStack, 0.8f), pos + new Vector2(-570, -370), Color.White, 0.8f);
        }
    }
}
