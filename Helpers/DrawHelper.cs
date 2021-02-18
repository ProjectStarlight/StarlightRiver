using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using StarlightRiver.Core;
using System.Runtime.InteropServices;

namespace StarlightRiver.Helpers
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

        private readonly Effect UpscaleEffect = Main.dedServ ? null : Filters.Scene["LightShader"].GetShader().Shader;

        private void RenderLightingQuad()
        {
            if (UpscaleEffect is null) return;

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

            Vector2 offset = (Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f - tileLightingCenter - Vector2.One * 64) / new Vector2(Main.screenWidth, Main.screenHeight);

            UpscaleEffect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            UpscaleEffect.Parameters["fullBufferSize"].SetValue(tileLightingTexture.Size() * 16);
            UpscaleEffect.Parameters["offset"].SetValue(offset);
            UpscaleEffect.Parameters["sampleTexture"].SetValue(tileLightingTexture);

            foreach (EffectPass pass in UpscaleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        int thetimer = 0;
        public void DebugDraw()
        {
            thetimer++;
            if (thetimer % 5 == 0) PopulateTileTexture((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * padding * 16);
            PopulateScreenTexture();
        }

        public void DebugDraw2()
        {
            Main.spriteBatch.Draw(tileLightingTexture, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
            Main.spriteBatch.Draw(screenLightingTexture, new Rectangle(Main.screenWidth / 2, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
            Main.spriteBatch.Draw(Main.screenTarget, new Rectangle(0, Main.screenHeight / 2, Main.screenWidth / 2, Main.screenHeight / 2), Color.White);
            Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth / 2, Main.screenHeight / 2), Color.Black);
            //Main.spriteBatch.Draw(screenLightingTexture, Vector2.Zero, Color.White);
        }
    }


    public static partial class Helper
    {
        private static readonly Effect ApplyEffect = Main.dedServ ? null : Filters.Scene["LightApply"].GetShader().Shader;

        private static readonly VertexPositionTexture[] verticies = new VertexPositionTexture[6];

        private static readonly VertexBuffer buffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionTexture), 6, BufferUsage.WriteOnly);

        public static void DrawWithLighting(Vector2 pos, Texture2D tex, Color color = default)
        {
            if (Main.dedServ || !OnScreen(new Rectangle((int)pos.X, (int)pos.Y, tex.Width, tex.Height))) return;
            if (color == default) color = Color.White;

            Matrix zoom = //Main.GameViewMatrix.ZoomMatrix;
                new Matrix
                (
                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                    0, Main.GameViewMatrix.Zoom.X, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );

            ApplyEffect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            ApplyEffect.Parameters["texSize"].SetValue(tex.Size());
            ApplyEffect.Parameters["offset"].SetValue(pos / new Vector2(Main.screenWidth, Main.screenHeight));
            ApplyEffect.Parameters["zoom"].SetValue(zoom);
            ApplyEffect.Parameters["drawColor"].SetValue(color.ToVector4());

            ApplyEffect.Parameters["targetTexture"].SetValue(tex);
            ApplyEffect.Parameters["sampleTexture"].SetValue(StarlightRiver.lightingTest.screenLightingTexture);

            verticies[0] = new VertexPositionTexture(new Vector3(ConvertX(pos.X), ConvertY(pos.Y), 0), Vector2.Zero);
            verticies[1] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + tex.Width), ConvertY(pos.Y), 0), Vector2.UnitX);
            verticies[2] = new VertexPositionTexture(new Vector3(ConvertX(pos.X), ConvertY(pos.Y + tex.Height), 0), Vector2.UnitY);

            verticies[3] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + tex.Width), ConvertY(pos.Y), 0), Vector2.UnitX);
            verticies[4] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + tex.Width), ConvertY(pos.Y + tex.Height), 0), Vector2.One);
            verticies[5] = new VertexPositionTexture(new Vector3(ConvertX(pos.X), ConvertY(pos.Y + tex.Height), 0), Vector2.UnitY);

            buffer.SetData(verticies);

            Main.instance.GraphicsDevice.SetVertexBuffer(buffer);

            foreach (EffectPass pass in ApplyEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

            Main.instance.GraphicsDevice.SetVertexBuffer(null);
        }

        public static void DrawWithLighting(Vector2 pos, Texture2D tex, Rectangle source, Color color = default) //just going to make this an overload for now. TODO: optional param conversion?
        {
            if (Main.dedServ || !OnScreen(new Rectangle((int)pos.X, (int)pos.Y, tex.Width, tex.Height))) return;
            if (color == default) color = Color.White;

            Matrix zoom = //Main.GameViewMatrix.ZoomMatrix;
                new Matrix
                (
                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                    0, Main.GameViewMatrix.Zoom.X, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );

            ApplyEffect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            ApplyEffect.Parameters["texSize"].SetValue(tex.Size());
            ApplyEffect.Parameters["offset"].SetValue(pos / new Vector2(Main.screenWidth, Main.screenHeight));
            ApplyEffect.Parameters["zoom"].SetValue(zoom);
            ApplyEffect.Parameters["drawColor"].SetValue(color.ToVector4());

            ApplyEffect.Parameters["targetTexture"].SetValue(tex);
            ApplyEffect.Parameters["sampleTexture"].SetValue(StarlightRiver.lightingTest.screenLightingTexture);

            verticies[0] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + source.X), ConvertY(pos.Y + source.Y), 0), source.TopLeft() / tex.Size());
            verticies[1] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + source.X + source.Width), ConvertY(pos.Y + source.Y), 0), source.TopLeft() / tex.Size() + Vector2.UnitX * source.Width / tex.Width);
            verticies[2] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + source.X), ConvertY(pos.Y + source.Y + source.Height), 0), source.TopLeft() / tex.Size() + Vector2.UnitY * source.Height / tex.Height);

            verticies[3] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + source.X + source.Width), ConvertY(pos.Y + source.Y), 0), source.TopLeft() / tex.Size() + Vector2.UnitX * source.Width / tex.Width);
            verticies[4] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + source.X + source.Width), ConvertY(pos.Y + source.Y + source.Height), 0), source.BottomRight() / tex.Size());
            verticies[5] = new VertexPositionTexture(new Vector3(ConvertX(pos.X + source.X), ConvertY(pos.Y + source.Y + source.Height), 0), source.TopLeft() / tex.Size() + Vector2.UnitY * source.Height / tex.Height);

            buffer.SetData(verticies);

            Main.instance.GraphicsDevice.SetVertexBuffer(buffer);

            foreach (EffectPass pass in ApplyEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

            Main.instance.GraphicsDevice.SetVertexBuffer(null);
        }

        private static readonly BasicEffect basicEffect = Main.dedServ ? null : new BasicEffect(Main.graphics.GraphicsDevice);

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

        public static void DrawElectricity(Vector2 point1, Vector2 point2, int dusttype, float scale = 1, int armLength = 30, Color color = default)
        {
            int nodeCount = (int)Vector2.Distance(point1, point2) / armLength;
            Vector2[] nodes = new Vector2[nodeCount + 1];

            nodes[nodeCount] = point2; //adds the end as the last point

            for (int k = 1; k < nodes.Count(); k++)
            {
                //Sets all intermediate nodes to their appropriate randomized dot product positions
                nodes[k] = Vector2.Lerp(point1, point2, k / (float)nodeCount) +
                    (k == nodes.Count() - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * Main.rand.NextFloat(-armLength / 2, armLength / 2));

                //Spawns the dust between each node
                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];
                for (float i = 0; i < 1; i += 0.05f)
                {
                    Dust.NewDustPerfect(Vector2.Lerp(prevPos, nodes[k], i), dusttype, Vector2.Zero, 0, color, scale);
                }
            }
        }

        public static float ConvertX(float input) => input / (Main.screenWidth / 2) - 1;

        public static float ConvertY(float input) => -1 * (input / (Main.screenHeight / 2) - 1);
    }

    public class Primitives : IDisposable 
    {
        public bool IsDisposed { get; private set; }

        private readonly DynamicVertexBuffer vertexBuffer;
        private readonly DynamicIndexBuffer indexBuffer;

        private readonly GraphicsDevice device;

        public Primitives(GraphicsDevice device, int maxVertices, int maxIndices)
        {
            this.device = device;

            vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColorTexture), maxVertices, BufferUsage.None);
            indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, maxIndices, BufferUsage.None);
        }

        public void Render(Effect effect)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }

        public void SetVertices(VertexPositionColorTexture[] vertices)
        {
            vertexBuffer.SetData(0, vertices, 0, vertices.Length, VertexPositionColorTexture.VertexDeclaration.VertexStride, SetDataOptions.Discard);
        }

        public void SetIndices(short[] indices)
        {
            indexBuffer.SetData(0, indices, 0, indices.Length, SetDataOptions.Discard);
        }

        public void Dispose()
        {
            IsDisposed = true;

            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }
    }
}
