using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Keys;
using System;
using System.Linq;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
	class DrawLayers : HookGroup
    {
        //A few different hooks for drawing on certain layers. Orig is always run and its just draw calls.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            //Under water
            On.Terraria.Main.drawWaters += DrawUnderwaterNPCs;
            //Keys
            On.Terraria.Main.DrawItems += DrawKeys;
            //Tile draws infront of the player
            On.Terraria.Main.DrawPlayer += PostDrawPlayer;
        }

        private void DrawUnderwaterNPCs(On.Terraria.Main.orig_drawWaters orig, Main self, bool bg, int styleOverride, bool allowUpdate) //TODO: Generalize this for later use
        {
            orig(self, bg, styleOverride, allowUpdate);
        }

        private void PostDrawPlayer(On.Terraria.Main.orig_DrawPlayer orig, Main self, Player drawPlayer, Vector2 Position, float rotation, Vector2 rotationOrigin, float shadow) //TODO: Generalize this for later use, and possibly optimize it also
        {
            if(!Main.gameMenu && shadow == 0)
                drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

            orig(self, drawPlayer, Position, rotation, rotationOrigin, shadow);

            if (!Main.gameMenu && shadow == 0)
                drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
        }

        private void DrawKeys(On.Terraria.Main.orig_DrawItems orig, Main self)
        {
            foreach (Key key in StarlightWorld.Keys)
                key.Draw(Main.spriteBatch);

            orig(self);
        }
    }
}