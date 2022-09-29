﻿using Microsoft.Xna.Framework;
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

            //On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayer += PostDrawPlayer;
            //On.Terraria.Graphics.Renderers.ReturnGatePlayerRenderer.DrawPlayer += PostDrawPlayerGate;

            On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_RenderAllLayers += PostDrawPlayerLayer;
        }

		private void PostDrawPlayerLayer(On.Terraria.DataStructures.PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref Terraria.DataStructures.PlayerDrawSet drawinfo)
		{
            var drawPlayer = drawinfo.drawPlayer;
            var shadow = drawinfo.shadow;

            if (!Main.gameMenu && shadow == 0) 
                drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

            orig(ref drawinfo);

            if (!Main.gameMenu && shadow == 0)
                drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
        }

		private void PostDrawPlayer(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayer orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
		{
            if (!Main.gameMenu && shadow == 0) 
                drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

            orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

            if (!Main.gameMenu && shadow == 0)
                drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
        }

        private void PostDrawPlayerGate(On.Terraria.Graphics.Renderers.ReturnGatePlayerRenderer.orig_DrawPlayer orig, object self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
        {
            if (!Main.gameMenu && shadow == 0)
                drawPlayer.GetModPlayer<StarlightPlayer>().PreDraw(drawPlayer, Main.spriteBatch);

            orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

            if (!Main.gameMenu && shadow == 0)
                drawPlayer.GetModPlayer<StarlightPlayer>().PostDraw(drawPlayer, Main.spriteBatch);
        }

        private void DrawKeys(On.Terraria.Main.orig_DrawItems orig, Main self)
        {
            foreach (Key key in KeySystem.Keys)
                key.Draw(Main.spriteBatch);

            orig(self);
        }
    }
}