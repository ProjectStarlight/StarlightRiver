using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
    class PlayerTarget : HookGroup
    {
        //Drawing Player to Target. Should be safe. Excuse me if im duplicating something that alr exists :p
        public override SafetyLevel Safety => SafetyLevel.Safe;

        private MethodInfo playerDrawMethod;

        public static RenderTarget2D Target;

        public static bool canUseTarget = false;

        public static RenderTarget2D ScaledTileTarget { get; set; }

        public static int sheetSquareX;
        public static int sheetSquareY;

        /// <summary>
        /// we use a dictionary for the player indexes because they are not guarenteed to be 0, 1, 2 etc. the player at index 1 leaving could result in 2 players being numbered 0, 2
        /// but we don't want a gigantic RT with all 255 possible players getting template space so we resize and keep track of their effective index
        /// </summary>
        private static Dictionary<int, int> playerIndexLookup;

        /// <summary>
        /// to keep track of player counts as they change
        /// </summary>
        private static int prevNumPlayers;


        //stored vars so we can determine original lighting for the player / potentially other uses
        Vector2 oldPos;
        Vector2 oldCenter;
        Vector2 oldMountedCenter;
        Vector2 oldScreen;
        Vector2 oldItemLocation;
        Vector2 positionOffset;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            sheetSquareX = 200;
            sheetSquareY = 300;

            playerIndexLookup = new Dictionary<int, int>();
            prevNumPlayers = -1;

            Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            ScaledTileTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

            playerDrawMethod = typeof(Main).GetMethod("DrawPlayer", BindingFlags.Public | BindingFlags.Instance);

            On.Terraria.Main.SetDisplayMode += RefreshTargets;
            On.Terraria.Main.CheckMonoliths += DrawTargets;
            On.Terraria.Lighting.GetColor_int_int += getColorOverride;
            On.Terraria.Lighting.GetColor_int_int_Color += getColorOverride;
        }
        public Color getColorOverride(On.Terraria.Lighting.orig_GetColor_int_int orig, int x, int y)
        {
            if (canUseTarget)
                return orig.Invoke(x, y);

            return orig.Invoke(x + (int)((oldPos.X - positionOffset.X) / 16), y + (int)((oldPos.Y - positionOffset.Y) / 16));
        }

        public Color getColorOverride(On.Terraria.Lighting.orig_GetColor_int_int_Color orig, int x, int y, Color c)
        {
            if (canUseTarget)
                return orig.Invoke(x, y, c);

            return orig.Invoke(x + (int)((oldPos.X - positionOffset.X) / 16), y + (int)((oldPos.Y - positionOffset.Y) / 16), c);
        }


        public static Rectangle getPlayerTargetSourceRectangle(int whoAmI)
        {
            if (playerIndexLookup.ContainsKey(whoAmI))
                return new Rectangle(playerIndexLookup[whoAmI] * sheetSquareX, 0, sheetSquareX, sheetSquareY);

            return Rectangle.Empty;
        }


        /// <summary>
        /// gets the whoAmI's player's renderTarget and returns a Vector2 that represents the rendertarget's position overlapping with the player's position in terms of screen coordinates
        /// </summary>
        /// <param name="whoAmI"></param>
        /// <returns></returns>
        public static Vector2 getPlayerTargetPosition(int whoAmI)
        {
            return Main.player[whoAmI].position - Main.screenPosition - new Vector2(sheetSquareX / 2, sheetSquareY / 2);
        }



        private void RefreshTargets(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (!Main.gameInactive && (width != Main.screenWidth || height != Main.screenHeight))
            {
                ScaledTileTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height);
            }

            orig(width, height, fullscreen);
        }

        private void DrawTargets(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            //TODO: this may benefit from adding booleans for other places in the code to check if they're going to use the RTs since we don't necessarily need these generated on every frame for some performance improvements

            orig();

            if (Main.gameMenu)
                return;

            if (Main.ActivePlayersCount > 0)
                DrawPlayerTarget();

            if (Main.instance.tileTarget.IsDisposed)
                return;

            RenderTargetBinding[] oldtargets1 = Main.graphics.GraphicsDevice.GetRenderTargets();

            Matrix matrix = Main.GameViewMatrix.ZoomMatrix;

            GraphicsDevice GD = Main.graphics.GraphicsDevice;
            SpriteBatch sb = Main.spriteBatch;

            GD.SetRenderTarget(ScaledTileTarget);
            GD.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            Main.spriteBatch.Draw(Main.instance.tileTarget, Main.sceneTilePos - Main.screenPosition, Color.White);
            sb.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets1);

        }

        public static Vector2 getPositionOffset(int whoAmI)
        {
            if (playerIndexLookup.ContainsKey(whoAmI))
                return new Vector2(playerIndexLookup[whoAmI] * sheetSquareX + sheetSquareX / 2, sheetSquareY / 2);

            return Vector2.Zero;
        }

        private void DrawPlayerTarget()
        {
            if (Main.ActivePlayersCount != prevNumPlayers)
            {
                prevNumPlayers = Main.ActivePlayersCount;
                Target = new RenderTarget2D(Main.graphics.GraphicsDevice, 300 * Main.ActivePlayersCount, 300);
                int activeCount = 0;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        playerIndexLookup[i] = activeCount;
                        activeCount++;
                    }

                }
            }

            RenderTargetBinding[] oldtargets2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            canUseTarget = false;
            Main.graphics.GraphicsDevice.SetRenderTarget(Target);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin();
            //Player drawPlayer, Vector2 Position, float rotation, Vector2 rotationOrigin, float shadow = 0f;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && Main.player[i].dye.Length > 0)
                {
                    oldPos = Main.player[i].position;
                    oldCenter = Main.player[i].Center;
                    oldMountedCenter = Main.player[i].MountedCenter;
                    oldScreen = Main.screenPosition;
                    oldItemLocation = Main.player[i].itemLocation;
                    int oldHeldProj = Main.player[i].heldProj;

                    //temp change player's actual position to lock into their frame
                    positionOffset = getPositionOffset(i);
                    Main.player[i].position = positionOffset;
                    Main.player[i].Center = oldCenter - oldPos + positionOffset;
                    Main.player[i].itemLocation = oldItemLocation - oldPos + positionOffset;
                    Main.player[i].MountedCenter = oldMountedCenter - oldPos + positionOffset;
                    Main.player[i].heldProj = -1;
                    Main.screenPosition = Vector2.Zero;
                    playerDrawMethod?.Invoke(Main.instance, new object[] { Main.player[i], Main.player[i].position, Main.player[i].fullRotation, Main.player[i].fullRotationOrigin, 0f });

                    Main.player[i].position = oldPos;
                    Main.player[i].Center = oldCenter;
                    Main.screenPosition = oldScreen;
                    Main.player[i].itemLocation = oldItemLocation;
                    Main.player[i].MountedCenter = oldMountedCenter;
                    Main.player[i].heldProj = oldHeldProj;
                }

            }


            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets2);
            canUseTarget = true;
        }
    }
}