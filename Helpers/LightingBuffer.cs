using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using StarlightRiver.Core;
using System.Runtime.InteropServices;
using Terraria.ModLoader;
using StarlightRiver.Configs;
using StarlightRiver.Physics;
using static StarlightRiver.Helpers.DrawHelper;

namespace StarlightRiver.Helpers
{
    public class LightingBuffer
    {
        const int PADDING = 20;
        static readonly float factor = Main.screenHeight / (float)Main.screenWidth;

        public static bool GettingColors = false;

        public RenderTarget2D ScreenLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public RenderTarget2D TileLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, XMax, YMax, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public Vector2 TileLightingCenter;

        private readonly Effect upscaleEffect = Main.dedServ ? null : Filters.Scene["LightShader"].GetShader().Shader;

        private Config config = ModContent.GetInstance<Config>();
        //public int RefreshRate = 5;
        private int refreshTimer;

        static int XMax => Main.screenWidth / 16 + PADDING * 2;
        static int YMax => (int)(Main.screenHeight / 16 + (PADDING * 2 * factor));

        public void ResizeBuffers(int width, int height)
        {
            float factor = height / (float)width;
            ScreenLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            TileLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, width / 16 + PADDING * 2, (int)(height / 16 + (PADDING * 2 * factor)), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private void PopulateTileTexture(Vector2 start)
        {
            GettingColors = true;
            Color[] tileLightingBuffer = new Color[TileLightingTexture.Width * TileLightingTexture.Height];

            for (int x = 0; x < TileLightingTexture.Width; x++)
                for (int y = 0; y < TileLightingTexture.Height; y++)
                {
                    int index = y * TileLightingTexture.Width + x;
                    if (tileLightingBuffer.Length > index) tileLightingBuffer[index] = Lighting.GetColor((int)start.X / 16 + x, (int)start.Y / 16 + y);
                }

            TileLightingTexture.SetData(tileLightingBuffer);
            TileLightingCenter = start + new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            GettingColors = false;
        }

        public void PopulateScreenTexture()
        {
            if (TileLightingTexture is null) 
                return;

            GraphicsDevice graphics = Main.instance.GraphicsDevice;

            graphics.SetRenderTarget(ScreenLightingTexture);

            graphics.Clear(Color.Black);
            RenderLightingQuad();

            graphics.SetRenderTarget(null);
        }

        private void RenderLightingQuad()
        {
            if (upscaleEffect is null) return;

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

            Vector2 offset = (Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f - TileLightingCenter - Vector2.One * 64) / new Vector2(Main.screenWidth, Main.screenHeight);

            upscaleEffect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            upscaleEffect.Parameters["fullBufferSize"].SetValue(TileLightingTexture.Size() * 16);
            upscaleEffect.Parameters["offset"].SetValue(offset);
            upscaleEffect.Parameters["sampleTexture"].SetValue(TileLightingTexture);

            foreach (EffectPass pass in upscaleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        public void DebugDraw()
        {
            if (ModContent.GetInstance<Config>().HighQualityLighting)
            {
                refreshTimer++;

                if (refreshTimer % config.LightingUpdateDelay == 0)
                    PopulateTileTexture((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * PADDING * 16);

                PopulateScreenTexture();
            }
        }
    }

    public static class LightingBufferRenderer
    {
        private static readonly Effect ApplyEffect = Main.dedServ ? null : Filters.Scene["LightApply"].GetShader().Shader;

        private static readonly VertexPositionTexture[] verticies = new VertexPositionTexture[6];
        private static readonly VertexPositionColorTexture[] verticiesColor = new VertexPositionColorTexture[12];

        private static readonly VertexBuffer buffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionTexture), 6, BufferUsage.WriteOnly);
        private static readonly VertexBuffer bufferColor = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 12, BufferUsage.WriteOnly);

        public static void DrawWithLighting(Matrix zoom, Rectangle pos, Texture2D tex, Rectangle source, Color color = default)
        {
            if (Main.dedServ || !Helper.OnScreen(new Rectangle(pos.X, pos.Y, tex.Width, tex.Height)))
                return;
            if (color == default)
                color = Color.White;

            if (!ModContent.GetInstance<Config>().HighQualityLighting)
            {
                Rectangle checkZone = Rectangle.Intersect(pos, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));
                Color topLeftColor = Lighting.GetColor((checkZone.X + (int)Main.screenPosition.X) / 16, (checkZone.Y + (int)Main.screenPosition.Y) / 16, color);
                Color topRightColor = Lighting.GetColor(((checkZone.X + checkZone.Width) + (int)Main.screenPosition.X) / 16, (checkZone.Y + (int)Main.screenPosition.Y) / 16, color);
                Color bottomLeftColor = Lighting.GetColor((checkZone.X + (int)Main.screenPosition.X) / 16, ((checkZone.Y + checkZone.Height) + (int)Main.screenPosition.Y) / 16, color);
                Color bottomRightColor = Lighting.GetColor(((checkZone.X + checkZone.Width) + (int)Main.screenPosition.X) / 16, ((checkZone.Y + checkZone.Height) + (int)Main.screenPosition.Y) / 16, color);
                Color centerColor = Lighting.GetColor((checkZone.Center.X + (int)Main.screenPosition.X) / 16, (checkZone.Center.Y + (int)Main.screenPosition.Y) / 16, color);

                verticiesColor[0] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopLeft()), 0),
                    topLeftColor, source.TopLeft() / tex.Size());
                verticiesColor[1] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopRight()), 0),
                    topRightColor, source.TopRight() / tex.Size());
                verticiesColor[2] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center()), 0),
                    centerColor, source.Center() / tex.Size());

                verticiesColor[3] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopRight()), 0),
                   topRightColor, source.TopRight() / tex.Size());
                verticiesColor[4] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomRight()), 0),
                    bottomRightColor, source.BottomRight() / tex.Size());
                verticiesColor[5] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center()), 0),
                    centerColor, source.Center() / tex.Size());

                verticiesColor[6] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center()), 0),
                    centerColor, source.Center() / tex.Size());
                verticiesColor[7] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomRight()), 0),
                    bottomRightColor, source.BottomRight() / tex.Size());
                verticiesColor[8] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomLeft()), 0),
                    bottomLeftColor, source.BottomLeft() / tex.Size());

                verticiesColor[9] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopLeft()), 0),
                    topLeftColor, source.TopLeft() / tex.Size());
                verticiesColor[10] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center()), 0),
                    centerColor, source.Center() / tex.Size());
                verticiesColor[11] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomLeft()), 0),
                    bottomLeftColor, source.BottomLeft() / tex.Size());

                basicEffect.TextureEnabled = true;
                basicEffect.VertexColorEnabled = true;
                basicEffect.Texture = tex;
                basicEffect.View = zoom;

                bufferColor.SetData(verticiesColor);
                Main.instance.GraphicsDevice.SetVertexBuffer(bufferColor);

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 4);
                }
            }
            else
            {
                ApplyEffect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                ApplyEffect.Parameters["texSize"].SetValue(tex.Size());
                ApplyEffect.Parameters["offset"].SetValue((pos.TopLeft() - source.TopLeft()) / new Vector2(Main.screenWidth, Main.screenHeight));
                ApplyEffect.Parameters["zoom"].SetValue(zoom);
                ApplyEffect.Parameters["drawColor"].SetValue(color.ToVector4());

                ApplyEffect.Parameters["targetTexture"].SetValue(tex);
                ApplyEffect.Parameters["sampleTexture"].SetValue(StarlightRiver.LightingBufferInstance.ScreenLightingTexture);

                verticies[0] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.TopLeft()), 0), source.TopLeft() / tex.Size());
                verticies[1] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.TopRight()), 0), source.TopRight() / tex.Size());
                verticies[2] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.BottomLeft()), 0), source.BottomLeft() / tex.Size());

                verticies[3] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.TopRight()), 0), source.TopRight() / tex.Size());
                verticies[4] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.BottomRight()), 0), source.BottomRight() / tex.Size());
                verticies[5] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.BottomLeft()), 0), source.BottomLeft() / tex.Size());

                /*verticies[0] = new VertexPositionTexture(new Vector3(DrawHelper.ConvertX(pos.X + source.X), DrawHelper.ConvertY(pos.Y + source.Y), 0), source.TopLeft() / tex.Size());
                  verticies[1] = new VertexPositionTexture(new Vector3(DrawHelper.ConvertX(pos.X + source.X + source.Width), DrawHelper.ConvertY(pos.Y + source.Y), 0), source.TopLeft() / tex.Size() + Vector2.UnitX * source.Width / tex.Width);
                  verticies[2] = new VertexPositionTexture(new Vector3(DrawHelper.ConvertX(pos.X + source.X), DrawHelper.ConvertY(pos.Y + source.Y + source.Height), 0), source.TopLeft() / tex.Size() + Vector2.UnitY * source.Height / tex.Height);

                  verticies[3] = new VertexPositionTexture(new Vector3(DrawHelper.ConvertX(pos.X + source.X + source.Width), DrawHelper.ConvertY(pos.Y + source.Y), 0), source.TopLeft() / tex.Size() + Vector2.UnitX * source.Width / tex.Width);
                  verticies[4] = new VertexPositionTexture(new Vector3(DrawHelper.ConvertX(pos.X + source.X + source.Width), DrawHelper.ConvertY(pos.Y + source.Y + source.Height), 0), source.BottomRight() / tex.Size());
                  verticies[5] = new VertexPositionTexture(new Vector3(DrawHelper.ConvertX(pos.X + source.X), DrawHelper.ConvertY(pos.Y + source.Y + source.Height), 0), source.TopLeft() / tex.Size() + Vector2.UnitY * source.Height / tex.Height);*/

                buffer.SetData(verticies);

                Main.instance.GraphicsDevice.SetVertexBuffer(buffer);

                foreach (EffectPass pass in ApplyEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                }
            }

            Main.instance.GraphicsDevice.SetVertexBuffer(null);
        }

        public static void DrawWithLightingWorld(Vector2 pos, Texture2D tex, Color color = default) =>
            DrawWithLightingWorld(pos, tex, tex.Frame(), color);

        public static void DrawWithLightingWorld(Vector2 pos, Texture2D tex, Rectangle source, Color color = default) =>
            DrawWithLightingWorld(new Rectangle((int)pos.X, (int)pos.Y, source.Width, source.Height), tex, source, color);

        public static void DrawWithLightingWorld(Rectangle pos, Texture2D tex, Rectangle source, Color color = default) =>
            DrawWithLighting(new Matrix
                                (
                                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                                    0, Main.GameViewMatrix.Zoom.Y, 0, 0,
                                    0, 0, 1, 0,
                                    0, 0, 0, 1
                                ), pos, tex, source, color);


        public static void DrawWithLightingTile(Vector2 pos, Texture2D tex, Color color = default) =>
            DrawWithLightingTile(pos, tex, tex.Frame(), color);

        public static void DrawWithLightingTile(Vector2 pos, Texture2D tex, Rectangle source, Color color = default) =>
            DrawWithLightingTile(new Rectangle((int)pos.X, (int)pos.Y, source.Width, source.Height), tex, source, color);
        //todo fix matrix
        public static void DrawWithLightingTile(Rectangle pos, Texture2D tex, Rectangle source, Color color = default) =>
            DrawWithLighting(Matrix.CreateScale(new Vector3(0.7845f, 0.67f, 1)), new Rectangle(pos.X - 194, pos.Y - 192, pos.Width, pos.Height), tex, source, color);
    }
}
