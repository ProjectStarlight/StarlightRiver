﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Astroflora;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	public class AstrofloraBow : HookGroup
    {
        // Not really any hacky stuff, its just detours with drawing. The only way this is crashing is if something else messes with the state of SB.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        private const string cursorTexture = AssetDirectory.Astroflora + "AstrofloraCrosshair";

        public override void Load()
        {
            On.Terraria.Main.DrawInterface_36_Cursor += orig =>
            {
                if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Items.Astroflora.AstrofloraBow>())
                {
                    Texture2D crosshair = ModContent.Request<Texture2D>(cursorTexture).Value;

                    Color color = (Main.LocalPlayer.HeldItem.ModItem as Items.Astroflora.AstrofloraBow).CursorShouldBeRed ? Color.Red : Main.cursorColor;

                    Main.spriteBatch.Draw(crosshair, Main.MouseScreen - (crosshair.Size() / 2), null, color, 0, Vector2.Zero, Main.cursorScale, SpriteEffects.None, 0);

                    return;
                }

                orig();
            };

            // Detoured to try and minimise batches.
            On.Terraria.Main.DrawNPCs += (orig, self, behindTiles) =>
            {
                orig(self, behindTiles);

                Effect effect = Filters.Scene["DonutIcon"].GetShader().Shader;

                if (effect is null)
                    return;

                effect.Parameters["upperRadiusLimit"].SetValue(0.5f);
                effect.Parameters["lowerRadiusLimit"].SetValue(0.375f);
                effect.Parameters["color"].SetValue(new Color(31, 250, 131).ToVector4());

                Texture2D invisible = ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value;

                Texture2D crosshair = ModContent.Request<Texture2D>(cursorTexture).Value;

                Color crosshairColor = Color.Lerp(Color.White, Main.cursorColor, (float)(Math.Sin(Main.timeForVisualEffects * 10) + 1) / 2);

                Vector2 sizeOfDonut = Vector2.One * 32;

                List<(NPC, float)> drawInfo = new List<(NPC, float)>();

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC NPC = Main.npc[i];

                    if (!NPC.active)
                    {
                        continue;
                    }

                    AstrofloraLocksGlobalNPC globalNPC = NPC.GetGlobalNPC<AstrofloraLocksGlobalNPC>();

                    if (globalNPC.Locked)
                    {
                        drawInfo.Add(new ValueTuple<NPC, float>(NPC, globalNPC.remainingLockDuration / (float)AstrofloraLocksGlobalNPC.MaxLockDuration));
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                foreach ((NPC, float) info in drawInfo)
                {
                    NPC NPC = info.Item1;

                    Vector2 aboveNpcTop = NPC.Center - new Vector2(sizeOfDonut.X / 2, (NPC.height / 2) + sizeOfDonut.Y) - Main.screenPosition;

                    Rectangle destinationRectangle = new Rectangle((int)aboveNpcTop.X, (int)aboveNpcTop.Y, (int)sizeOfDonut.X, (int)sizeOfDonut.Y);

                    effect.Parameters["time"].SetValue(info.Item2);

                    Main.spriteBatch.Draw(invisible, destinationRectangle, Color.White);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            };
        }
    }
}
