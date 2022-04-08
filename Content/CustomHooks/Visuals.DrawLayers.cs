using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.Overgrow;
using StarlightRiver.Core;
using StarlightRiver.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics;
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

            //Keys
            On.Terraria.Main.DrawItems += DrawKeys;
            //Tile draws infront of the Player
            On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayer += PostDrawPlayer;
        }

		private void PostDrawPlayer(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayer orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
		{
            if (!Main.gameMenu && shadow == 0) //PORTTODO: Maybe pass through more of these params to these methods?
                drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

            orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

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