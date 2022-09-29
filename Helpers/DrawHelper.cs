﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Helpers
{
	public static class DrawHelper
	{
        public static readonly BasicEffect basicEffect = Main.dedServ ? null : new BasicEffect(Main.graphics.GraphicsDevice);

        public static Vector2 PointAccur(this Vector2 input) => input.ToPoint().ToVector2();

        public static float ConvertX(float input) => input / (Main.screenWidth * 0.5f) - 1;

        public static float ConvertY(float input) => -1 * (input / (Main.screenHeight * 0.5f) - 1);

        public static Vector2 ConvertVec2(Vector2 input) => new Vector2(1, -1) * (input / (new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f) - Vector2.One);

        public static void DrawHitbox(this SpriteBatch spriteBatch, NPC NPC, Color color) => spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, NPC.getRect().WorldToScreenCoords(), color);

        public static Rectangle WorldToScreenCoords(this Rectangle rect) => new Rectangle(rect.X - (int)Main.screenPosition.X, rect.Y - (int)Main.screenPosition.Y, rect.Width, rect.Height);

        public static void DrawTriangle(Texture2D tex, Vector2[] target, Vector2[] source)
        {
            if (basicEffect is null) return;

            basicEffect.TextureEnabled = true;
            basicEffect.Texture = tex;
            basicEffect.Alpha = 1;
            basicEffect.View = new Matrix
                (
                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                    0, Main.GameViewMatrix.Zoom.X, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );

            var gd = Main.graphics.GraphicsDevice;
            var points = new VertexPositionTexture[3];
            var buffer = new VertexBuffer(gd, typeof(VertexPositionTexture), 3, BufferUsage.WriteOnly);

            for (int k = 0; k < 3; k++)
                points[k] = new VertexPositionTexture(new Vector3(ConvertX(target[k].X), ConvertY(target[k].Y), 0), source[k] / tex.Size());

            buffer.SetData(points);

            gd.SetVertexBuffer(buffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }

            gd.SetVertexBuffer(null);
        }

        public static void DrawLine(SpriteBatch spritebatch, Vector2 startPoint, Vector2 endPoint, Texture2D texture, Color color, Rectangle sourceRect = default)
        {
            Vector2 edge = endPoint - startPoint;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);

            Vector2 offsetStart = startPoint + new Vector2(0, -(sourceRect.Width / 2)).RotatedBy(angle);//multiply before adding to startpoint to make the points closer

            spritebatch.Draw(texture,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)offsetStart.X,
                    (int)offsetStart.Y,
                    (int)edge.Length(), //sb will stretch the texture to fill this rectangle
                    sourceRect.Width), //width of line, change this to make thicker line (may have to offset?)
                sourceRect,
                color, //colour of line
                angle, //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                default);
        }

        public static void DrawElectricity(Vector2 point1, Vector2 point2, int dusttype, float scale = 1, int armLength = 30, Color color = default, float frequency = 0.05f)
        {
            int nodeCount = (int)Vector2.Distance(point1, point2) / armLength;
            Vector2[] nodes = new Vector2[nodeCount + 1];

            nodes[nodeCount] = point2; //adds the end as the last point

            for (int k = 1; k < nodes.Length; k++)
            {
                //Sets all intermediate nodes to their appropriate randomized dot product positions
                nodes[k] = Vector2.Lerp(point1, point2, k / (float)nodeCount) +
                    (k == nodes.Length - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * Main.rand.NextFloat(-armLength / 2, armLength / 2));

                //Spawns the dust between each node
                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];
                for (float i = 0; i < 1; i += frequency)
                {
                    Dust.NewDustPerfect(Vector2.Lerp(prevPos, nodes[k], i), dusttype, Vector2.Zero, 0, color, scale);
                }
            }
        }

        /// <summary>
        /// Draws a flat colored block that respects slopes, for making multitile structures
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="spriteBatch"></param>
        public static void TileDebugDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            int height = tile.TileFrameY == 36 ? 18 : 16;
            if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height), Color.Magenta * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else if (tile.IsHalfBlock)
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition + new Vector2(0, 10), new Rectangle(tile.TileFrameX, tile.TileFrameY + 10, 16, 6), Color.Red * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else
            {
                byte b3 = (byte)tile.Slope;
                int num34;
                for (int num226 = 0; num226 < 8; num226 = num34 + 1)
                {
                    int num227 = num226 << 1;
                    Rectangle value5 = new Rectangle(tile.TileFrameX, tile.TileFrameY + num226 * 2, num227, 2);
                    int num228 = 0;
                    switch (b3)
                    {
                        case 2:
                            value5.X = 16 - num227;
                            num228 = 16 - num227;
                            break;
                        case 3:
                            value5.Width = 16 - num227;
                            break;
                        case 4:
                            value5.Width = 14 - num227;
                            value5.X = num227 + 2;
                            num228 = num227 + 2;
                            break;
                    }
                    spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition + new Vector2((float)num228, (num226 * 2)), value5, Color.Blue * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    num34 = num226;
                }
            }
        }
    }
}
