using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Configs;
using Terraria;
using Terraria.Graphics.Effects;
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

        public static VertexBuffer lightingQuadBuffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);

        public RenderTarget2D ScreenLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public RenderTarget2D TileLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, XMax, YMax, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        public RenderTarget2D TileLightingTempTexture;
        public Vector2 TileLightingCenter;

        private int refreshTimer;

        static int XMax => Main.screenWidth / 16 + PADDING * 2;
        static int YMax => (int)(Main.screenHeight / 16 + (PADDING * 2 * factor));

        private GraphicsConfig config => ModContent.GetInstance<GraphicsConfig>();

        public static void SetupLightingQuadBuffer()
        {
            VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[6];

            verticies[0] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            verticies[1] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));
            verticies[2] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

            verticies[3] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            verticies[4] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            verticies[5] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

            lightingQuadBuffer.SetData(verticies);
        }

        public void ResizeBuffers(int width, int height)
        {
            float factor = height / (float)width;
            ScreenLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            TileLightingTexture = new RenderTarget2D(Main.instance.GraphicsDevice, width / 16 + PADDING * 2, (int)(height / 16 + (PADDING * 2 * factor)), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            if(TileLightingTempTexture != null)
                TileLightingTempTexture = new RenderTarget2D(Main.instance.GraphicsDevice, XMax, YMax, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
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
            TileLightingCenter = start;
            GettingColors = false;
        }

        private void PopulateTileTextureScrolling(Vector2 start, int yToStart, int yToEnd)
        {
            GettingColors = true;
            Color[] tileLightingBuffer = new Color[TileLightingTempTexture.Width * (yToEnd - yToStart)];

            for (int x = 0; x < TileLightingTempTexture.Width; x++)
                for (int y = yToStart; y < yToEnd; y++)
                {
                    int index = (y - yToStart) * TileLightingTempTexture.Width + x;
                    if (tileLightingBuffer.Length > index) tileLightingBuffer[index] = Lighting.GetColor((int)start.X / 16 + x, (int)start.Y / 16 + y);
                }

            if (tileLightingBuffer is null || tileLightingBuffer.Length == 0)
                return;

            TileLightingTempTexture.SetData(0, new Rectangle(0, yToStart, TileLightingTempTexture.Width, yToEnd - yToStart), tileLightingBuffer, 0, TileLightingTempTexture.Width * (yToEnd - yToStart));

            if(refreshTimer % config.LightingPollRate == 0)
                TileLightingCenter = start;

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
            if (Main.dedServ) 
                return;

            Effect upscaleEffect = Filters.Scene["LightShader"].GetShader().Shader;

            GraphicsDevice graphics = Main.instance.GraphicsDevice;

            graphics.SetVertexBuffer(lightingQuadBuffer);
            graphics.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            Vector2 offset = (Main.screenPosition - TileLightingCenter) / new Vector2(Main.screenWidth, Main.screenHeight);

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
            if (ModContent.GetInstance<GraphicsConfig>().HighQualityLighting)
            {
                refreshTimer++;

                if (config.ScrollingLightingPoll)
                {
                    if (TileLightingTempTexture is null)
                        TileLightingTempTexture = new RenderTarget2D(Main.instance.GraphicsDevice, XMax, YMax, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                    int progress = refreshTimer % config.LightingPollRate;
                    PopulateTileTextureScrolling((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * PADDING * 16, (int)(progress / (float)config.LightingPollRate * TileLightingTexture.Height), (int)((progress + 1) / (float)config.LightingPollRate * TileLightingTexture.Height));

                    if (refreshTimer % config.LightingPollRate == 0)
                    {
                        Color[] colorData = new Color[TileLightingTexture.Width * TileLightingTexture.Height];
                        TileLightingTempTexture.GetData(colorData);
                        TileLightingTexture.SetData(colorData);
                    }
                }
                else
                {
                    //Trust me this check is somehow needed even tho the config shouldn't allow this to happen :p
                    if (config.LightingPollRate != 0)
                    {
                        if (refreshTimer % config.LightingPollRate == 0)
                            PopulateTileTexture((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * PADDING * 16);
                    }
                }

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

        public static void DrawWithLighting(Rectangle pos, Texture2D tex, Rectangle source, Color color = default)
        {
            if (Main.dedServ || !Helper.OnScreen(new Rectangle(pos.X, pos.Y, tex.Width, tex.Height)))
                return;

            if (color == default)
                color = Color.White;

            Matrix zoom =  //Main.GameViewMatrix.ZoomMatrix;
            new Matrix
            (
                Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                0, Main.GameViewMatrix.Zoom.X, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            if (!ModContent.GetInstance<GraphicsConfig>().HighQualityLighting)
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
                basicEffect.Alpha = color.A / 255f;//you could also mult every color (after getLighting) by this

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

                Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                foreach (EffectPass pass in ApplyEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                }
            }

            Main.instance.GraphicsDevice.SetVertexBuffer(null);
        }


        public static void DrawWithLighting(Vector2 pos, Texture2D tex, Rectangle source, Color color = default) =>
            DrawWithLighting(new Rectangle((int)pos.X, (int)pos.Y, source.Width, source.Height), tex, source, color);

        public static void DrawWithLighting(Vector2 pos, Texture2D tex, Color color = default) =>
            DrawWithLighting(pos, tex, tex.Frame(), color);
    }
}
