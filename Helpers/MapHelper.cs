using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using static Terraria.WorldGen;


namespace StarlightRiver.Helpers
{
    public static partial class Helper //Credit to GabeHasWon for this code
    {
        #region full map drawing
        public static void DrawMirrorOnFullscreenMap(int tileX, int tileY, bool isTarget, Texture2D tex)
        {
            float myScale = isTarget ? 0.25f : 0.125f;
            float uiScale = 5f;//Main.mapFullscreenScale;
            float scale = uiScale * myScale;

            int wldBaseX = ((tileX + 1) << 4) + 8;
            int wldBaseY = ((tileY + 1) << 4) + 8;
            var wldPos = new Vector2(wldBaseX, wldBaseY);

            var (ScreenPosition, IsOnScreen) = GetFullMapPositionAsScreenPosition(wldPos);

            if (IsOnScreen && tileX > 0 && tileY > 0)
            {
                Vector2 scrPos = ScreenPosition;
                Main.spriteBatch.Draw(
                    texture: tex,
                    position: scrPos,
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0f,
                    origin: new Vector2(tex.Width / 2, tex.Height / 2),
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            }
        }

        public static (Vector2 ScreenPosition, bool IsOnScreen) GetFullMapPositionAsScreenPosition(Vector2 worldPosition) => GetFullMapPositionAsScreenPosition(new Rectangle((int)worldPosition.X, (int)worldPosition.Y, 0, 0));

        public static (int Width, int Height) GetScreenSize()
        {
            int screenWid = (int)(Main.screenWidth / Main.GameZoomTarget);
            int screenHei = (int)(Main.screenHeight / Main.GameZoomTarget);

            return (screenWid, screenHei);
        }

        public static (Vector2 ScreenPosition, bool IsOnScreen) GetFullMapPositionAsScreenPosition(Rectangle worldArea)
        {    //Main.mapFullscreen
            float mapScale = GetFullMapScale();
            var scrSize = GetScreenSize();

            //float offscrLitX = 10f * mapScale;
            //float offscrLitY = 10f * mapScale;

            float mapFullscrX = Main.mapFullscreenPos.X * mapScale;
            float mapFullscrY = Main.mapFullscreenPos.Y * mapScale;
            float mapX = -mapFullscrX + (Main.screenWidth / 2f);
            float mapY = -mapFullscrY + (Main.screenHeight / 2f);

            float originMidX = (worldArea.X / 16f) * mapScale;
            float originMidY = (worldArea.Y / 16f) * mapScale;

            originMidX += mapX;
            originMidY += mapY;

            var scrPos = new Vector2(originMidX, originMidY);
            bool isOnscreen = originMidX >= 0 &&
                originMidY >= 0 &&
                originMidX < scrSize.Item1 &&
                originMidY < scrSize.Item2;

            return (scrPos, isOnscreen);
        }

        public static float GetFullMapScale() => Main.mapFullscreenScale / Main.UIScale;

        #endregion

        #region minimap drawing
        public static bool PointOnMinimap(Vector2 pos, float fluff = 4) => pos.X > (Main.miniMapX + fluff) && pos.X < (Main.miniMapX + Main.miniMapWidth - fluff) && pos.Y > (Main.miniMapY + fluff) && pos.Y < (Main.miniMapY + Main.miniMapHeight - fluff);

        /// <summary>Gets the position of something on the minimap given the tile coordinates.</summary>
        /// <param name="scale"></param>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public static Vector2 GetMiniMapPosition(Vector2 tilePos, float scale = 1f)
        {
            float screenHalfWidthInTiles = (Main.screenPosition.X + (PlayerInput.RealScreenWidth / 2f)) / 16f;
            float screenHalfHeightInTiles = (Main.screenPosition.Y + (PlayerInput.RealScreenHeight / 2f)) / 16f;
            float offsetX = screenHalfWidthInTiles - (Main.miniMapWidth / scale) / 2f;
            float offsetY = screenHalfHeightInTiles - (Main.miniMapHeight / scale) / 2f;

            float drawPosX = (tilePos.X - offsetX) * scale;
            float drawPosY = (tilePos.Y - offsetY) * scale;

            Vector2 miniMapSize = new Vector2(Main.miniMapX, Main.miniMapY);
            drawPosX += miniMapSize.X;
            drawPosY += miniMapSize.Y;
            drawPosY -= 2f * scale / 5f;

            return new Vector2(drawPosX, drawPosY);
        }

        public static void DrawOnMinimap(int posX, int posY, float scale, Texture2D tex)
        {
            Vector2 scrPos = new Vector2(posX, posY);
            Main.spriteBatch.Draw(
                texture: tex,
                position: scrPos,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: new Vector2(tex.Width / 2, tex.Height / 2),
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }

        #endregion
    }
}

