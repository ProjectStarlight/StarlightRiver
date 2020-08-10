using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Configs;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver
{
    public class RenderTest
    {
        const int padding = 20;
        static float factor = Main.screenHeight / (float)Main.screenWidth;

        static int XMax => Main.screenWidth / 16 + padding * 2;
        static int YMax => (int)(Main.screenHeight / 16 + (padding * 2 * factor));

        public static bool gettingColors = false;

        public RenderTarget2D screenLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public RenderTarget2D tileLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, XMax, YMax, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public Vector2 tileLightingCenter;

        private void ResizeBuffers()
        {
            factor = Main.screenHeight / (float)Main.screenWidth;
            screenLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            tileLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, XMax, YMax, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private void PopulateTileTexture(Vector2 start)
        {
            try
            {
                gettingColors = true;
                GraphicsDevice graphics = Main.instance.GraphicsDevice;
                Color[] tileLightingBuffer = new Color[(XMax) * (YMax)];

                for (int x = 0; x < XMax; x++)
                    for (int y = 0; y < YMax; y++)
                    {
                        int index = y * XMax + x;
                        if (tileLightingBuffer.Length > index) tileLightingBuffer[index] = Lighting.GetColor((int)start.X / 16 + x, (int)start.Y / 16 + y);
                    }

                tileLightingTexture.SetData(tileLightingBuffer);
                tileLightingCenter = start + new Vector2(Main.screenWidth, Main.screenHeight) / 2;
                gettingColors = false;
            }
            catch
            {
                ResizeBuffers();
            }
        }

        public void PopulateScreenTexture()
        {
            if (tileLightingTexture == null) return;

            GraphicsDevice graphics = Main.instance.GraphicsDevice;

            graphics.SetRenderTarget(screenLightingTexture);

            graphics.Clear(Color.Black);
            RenderLightingQuad();

            graphics.SetRenderTarget(null);
        }

        private void RenderLightingQuad()
        {
            GraphicsDevice graphics = Main.instance.GraphicsDevice;
            VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[6];

            verticies[0] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            verticies[1] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.Red, new Vector2(1, 1));
            verticies[2] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

            verticies[3] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            verticies[4] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.Red, new Vector2(0, 0));
            verticies[5] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

            VertexBuffer buffer = new VertexBuffer(graphics, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
            buffer.SetData(verticies);

            graphics.SetVertexBuffer(buffer);
            graphics.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            Vector2 offset = (Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f - tileLightingCenter) / new Vector2(Main.screenWidth, Main.screenHeight);
            float scale = 0.668f; //TODO: Figure out how to calculate this

            Effect someEffect = Filters.Scene["Lighting"].GetShader().Shader;
            someEffect.Parameters["uScreenResolution"].SetValue(new Vector2(scale, scale));
            someEffect.Parameters["mouse"].SetValue(offset);
            someEffect.Parameters["sampleTexture"].SetValue(tileLightingTexture);

            foreach (EffectPass pass in someEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        int thetimer = 0;
        public void DebugDraw(GameTime timer)
        {
            thetimer++;
            if(thetimer % 5 == 0) PopulateTileTexture((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * padding * 16);
            PopulateScreenTexture();
        }

        public void DebugDraw2()
        {
            Main.spriteBatch.Draw(tileLightingTexture, new Vector2(50, 150), Color.White);
            Main.spriteBatch.Draw(screenLightingTexture, new Rectangle(50, 160 + YMax, XMax, YMax), Color.White);
            //Main.spriteBatch.Draw(screenLightingTexture, Vector2.Zero, Color.White);
        }
    }


    public static partial class Helper
    {
        public static void DrawWithLighting(Vector2 pos, Texture2D tex)
        {
            return;
            if (!OnScreen(new Rectangle((int)pos.X, (int)pos.Y, tex.Width, tex.Height))) return;

            int coarseness = 1;
            int coarse16 = coarseness * 16;

            VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[(tex.Width / coarse16 + 1) * (tex.Height / coarse16 * 6 + 1)];

            Color[,] colorCache = new Color[tex.Width / coarse16 + 1, tex.Height / coarse16 + 2];
            Vector3[,] posCache = new Vector3[tex.Width / coarse16 + 1, tex.Height / coarse16 + 2];

            for (int x = 0; x < tex.Width / coarse16 + 1; x++) //populate the position/color arrays, so that they dont have to be re-calculated for each square
                for (int y = 0; y < tex.Height / coarse16 + 2; y++)
                {
                    Vector2 target = pos + new Vector2(x, y) * coarse16;
                    colorCache[x, y] = Lighting.GetColor((int)(target.X + Main.screenPosition.X) / 16, (int)(target.Y + Main.screenPosition.Y) / 16);
                    posCache[x, y] = new Vector3(ConvertX(target.X), ConvertY(target.Y), 0);
                }

            int targetIndex = 0;
            for (int x = 0; x < tex.Width; x += coarse16)
                for (int y = 0; y < tex.Height; y += coarse16)
                {
                    int xRel = x / coarse16;
                    int yRel = y / coarse16;

                    Color topLeft = colorCache[xRel, yRel];
                    Color topRight = colorCache[xRel + 1, yRel];
                    Color bottomLeft = colorCache[xRel, yRel + 1];
                    Color bottomRight = colorCache[xRel + 1, yRel + 1];

                    verticies[targetIndex] = (new VertexPositionColorTexture(posCache[xRel, yRel], topLeft, ConvertTex(new Vector2(x, y), tex))); targetIndex++;
                    verticies[targetIndex] = (new VertexPositionColorTexture(posCache[xRel + 1, yRel], topRight, ConvertTex(new Vector2(x + coarse16, y), tex))); targetIndex++;
                    verticies[targetIndex] = (new VertexPositionColorTexture(posCache[xRel + 1, yRel + 1], bottomRight, ConvertTex(new Vector2(x + coarse16, y + coarse16), tex))); targetIndex++;

                    verticies[targetIndex] = (new VertexPositionColorTexture(posCache[xRel, yRel + 1], bottomLeft, ConvertTex(new Vector2(x, y + coarse16), tex))); targetIndex++;
                    verticies[targetIndex] = (new VertexPositionColorTexture(posCache[xRel, yRel], topLeft, ConvertTex(new Vector2(x, y), tex))); targetIndex++;
                    verticies[targetIndex] = (new VertexPositionColorTexture(posCache[xRel + 1, yRel + 1], bottomRight, ConvertTex(new Vector2(x + coarse16, y + coarse16), tex))); targetIndex++;
                }
                
            if (verticies.Length >= 3) //cant draw a triangle with < 3 points fucktard
            {
                VertexBuffer buffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), verticies.Length, BufferUsage.WriteOnly);
                buffer.SetData(verticies);

                Main.instance.GraphicsDevice.SetVertexBuffer(buffer);

                BasicEffect basicEffect = new BasicEffect(Main.instance.GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = true,
                    Texture = tex
                };

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, verticies.Length / 3);
                }

                Main.instance.GraphicsDevice.SetVertexBuffer(null);
            }
        }

        private static float ConvertX(float input) => input / (Main.screenWidth / 2) - 1;

        private static float ConvertY(float input) => -1 * (input / (Main.screenHeight / 2) - 1);

        private static Vector2 ConvertTex(Vector2 input, Texture2D tex)
        {
            float x = input.X / tex.Width;
            float y = input.Y / tex.Height;
            return new Vector2(x, y);
        }
    }
}
