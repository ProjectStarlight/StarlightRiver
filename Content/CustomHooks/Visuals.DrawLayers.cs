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

            //Under water
            On.Terraria.Main.DrawWaters += DrawUnderwaterNPCs;
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

		private void DrawUnderwaterNPCs(On.Terraria.Main.orig_DrawWaters orig, Main self, bool bg) //TODO: Generalize this for later use
        {
            orig(self, bg);

            foreach (NPC NPC in Main.npc.Where(NPC => NPC.type == NPCType<BoneMine>() && NPC.active))
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                Color drawColor = Lighting.GetColor((int)NPC.position.X / 16, (int)NPC.position.Y / 16) * 0.3f;

                spriteBatch.Draw(Request<Texture2D>(NPC.ModNPC.Texture).Value, NPC.position - Main.screenPosition + Vector2.One * 16 * 12 + new Vector2((float)Math.Sin(NPC.ai[0]) * 4f, 0), drawColor);
                for (int k = 0; k >= 0; k++)
                {
                    if (Main.tile[(int)NPC.position.X / 16, (int)NPC.position.Y / 16 + k + 2].HasTile) break;
                    spriteBatch.Draw(Request<Texture2D>(AssetDirectory.OvergrowItem + "ShakerChain").Value,
                        NPC.Center - Main.screenPosition + Vector2.One * 16 * 12 + new Vector2(-4 + (float)Math.Sin(NPC.ai[0] + k) * 4, 18 + k * 16), drawColor);
                }
            }
        }

        private void DrawKeys(On.Terraria.Main.orig_DrawItems orig, Main self)
        {
            foreach (Key key in StarlightWorld.Keys)
                key.Draw(Main.spriteBatch);

            orig(self);
        }
    }
}