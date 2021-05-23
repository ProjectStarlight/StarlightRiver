using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System;
using static Terraria.ModLoader.ModContent;
using System.Reflection;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
    public partial class PrimTrail
    {
        public interface ITrailShader
        {
            string ShaderPass { get; }
            void ApplyShader<T>(Effect effect, T trail, List<Vector2> positions, string ESP, float progressParam);
        }
        public class DefaultShader : ITrailShader
        {
            public string ShaderPass => "DefaultPass";
            public void ApplyShader<T>(Effect effect, T trail, List<Vector2> positions, string ESP, float progressParam)
            {
                try
                {
                    effect.Parameters["progress"].SetValue(progressParam);
                    effect.CurrentTechnique.Passes[ESP].Apply();
                    effect.CurrentTechnique.Passes[ShaderPass].Apply();
                }
                catch
                {

                }

            }
        }
        protected static Vector2 CurveNormal(List<Vector2> points, int index)
        {
            if (points.Count == 1) return points[0];

            if (index == 0)
            {
                return Clockwise90(Vector2.Normalize(points[1] - points[0]));
            }
            if (index == points.Count - 1)
            {
                return Clockwise90(Vector2.Normalize(points[index] - points[index - 1]));
            }
            return Clockwise90(Vector2.Normalize(points[index + 1] - points[index - 1]));
        }
        protected static Vector2 Clockwise90(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }
        protected void PrepareShader(Effect effects, string PassName, float progress, Color? color = null)
        {
            int width = _device.Viewport.Width;
            int height = _device.Viewport.Height;
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(width / 2, height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
            Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);
            effects.Parameters["WorldViewProjection"].SetValue(view * projection);
            Color Color = color ?? Color.White;
            if (effects.HasParameter("uColor"))
            {
                effects.Parameters["uColor"].SetValue(Color.ToVector3());
            }

            _trailShader.ApplyShader(effects, this, _points, PassName, progress);
        }
        protected void PrepareBasicShader()
        {
            int width = _device.Viewport.Width;
            int height = _device.Viewport.Height;
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(width / 2, height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
            Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
        }
        protected void AddVertex(Vector2 position, Color color, Vector2 uv)
        {
            if (currentIndex < vertices.Length)
            {
                if (pixellated)
                    vertices[currentIndex++] = new VertexPositionColorTexture(new Vector3((position - Main.screenPosition) / 2, 0f), color, uv);
                else
                    vertices[currentIndex++] = new VertexPositionColorTexture(new Vector3((position - Main.screenPosition), 0f), color, uv);
            }
        }
        protected void MakePrimHelix(int i, int Width, float alphaValue, Color baseColour = default, float fadeValue = 1, float sineFactor = 0)
        {
            float _cap = (float)this._cap;
            Color c = (baseColour == default ? Color.White : baseColour) * (i / _cap) * fadeValue;
            Vector2 normal = CurveNormal(_points, i);
            Vector2 normalAhead = CurveNormal(_points, i + 1);
            float fallout = (float)Math.Sin(i * (3.14f / _cap));
            float fallout1 = (float)Math.Sin((i + 1) * (3.14f / _cap));
            float lerpers = _counter / 15f;
            float sine1 = i * (6.14f / _points.Count);
            float sine2 = (i + 1) * (6.14f / _cap);
            float width = Width * Math.Abs((float)Math.Sin(sine1 + lerpers) * (i / _cap)) * fallout;
            float width2 = Width * Math.Abs((float)Math.Sin(sine2 + lerpers) * ((i + 1) / _cap)) * fallout1;
            Vector2 firstUp = _points[i] - normal * width + new Vector2(0, (float)Math.Sin(_counter / 10f + i / 3f)) * sineFactor;
            Vector2 firstDown = _points[i] + normal * width + new Vector2(0, (float)Math.Sin(_counter / 10f + i / 3f)) * sineFactor;
            Vector2 secondUp = _points[i + 1] - normalAhead * width2 + new Vector2(0, (float)Math.Sin(_counter / 10f + (i + 1) / 3f)) * sineFactor;
            Vector2 secondDown = _points[i + 1] + normalAhead * width2 + new Vector2(0, (float)Math.Sin(_counter / 10f + (i + 1) / 3f)) * sineFactor;

            AddVertex(firstDown, c * alphaValue, new Vector2((i / _cap), 1));
            AddVertex(firstUp, c * alphaValue, new Vector2((i / _cap), 0));
            AddVertex(secondDown, c * alphaValue, new Vector2((i + 1) / _cap, 1));

            AddVertex(secondUp, c * alphaValue, new Vector2((i + 1) / _cap, 0));
            AddVertex(secondDown, c * alphaValue, new Vector2((i + 1) / _cap, 1));
            AddVertex(firstUp, c * alphaValue, new Vector2((i / _cap), 0));
        }
        protected void MakePrimMidFade(int i, int Width, float alphaValue, Color baseColour = default, float fadeValue = 1, float sineFactor = 0)
        {
            float _cap = (float)this._cap;
            Color c = (baseColour == default ? Color.White : baseColour) * (i / _cap) * fadeValue;
            Vector2 normal = CurveNormal(_points, i);
            Vector2 normalAhead = CurveNormal(_points, i + 1);
            float width = (i / _cap) * Width;
            float width2 = ((i + 1) / _cap) * Width;
            Vector2 firstUp = _points[i] - normal * width + new Vector2(0, (float)Math.Sin(_counter / 10f + i / 3f)) * sineFactor;
            Vector2 firstDown = _points[i] + normal * width + new Vector2(0, (float)Math.Sin(_counter / 10f + i / 3f)) * sineFactor;
            Vector2 secondUp = _points[i + 1] - normalAhead * width2 + new Vector2(0, (float)Math.Sin(_counter / 10f + (i + 1) / 3f)) * sineFactor;
            Vector2 secondDown = _points[i + 1] + normalAhead * width2 + new Vector2(0, (float)Math.Sin(_counter / 10f + (i + 1) / 3f)) * sineFactor;

            AddVertex(firstDown, c * alphaValue, new Vector2((i / _cap), 1));
            AddVertex(firstUp, c * alphaValue, new Vector2((i / _cap), 0));
            AddVertex(secondDown, c * alphaValue, new Vector2((i + 1) / _cap, 1));

            AddVertex(secondUp, c * alphaValue, new Vector2((i + 1) / _cap, 0));
            AddVertex(secondDown, c * alphaValue, new Vector2((i + 1) / _cap, 1));
            AddVertex(firstUp, c * alphaValue, new Vector2((i / _cap), 0));
        }
        protected void DrawBasicTrail(Color c1, float widthVar)
        {
            int currentIndex = 0;
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[_noOfPoints];
            for (int i = 0; i < _points.Count; i++)
            {
                if (i == 0)
                {

                    Vector2 normalAhead = CurveNormal(_points, i + 1);
                    Vector2 secondUp = _points[i + 1] - normalAhead * widthVar;
                    Vector2 secondDown = _points[i + 1] + normalAhead * widthVar;
                    AddVertex(_points[i], c1 * _alphaValue, new Vector2((float)Math.Sin(_counter / 20f), (float)Math.Sin(_counter / 20f)));
                    AddVertex(secondUp, c1 * _alphaValue, new Vector2((float)Math.Sin(_counter / 20f), (float)Math.Sin(_counter / 20f)));
                    AddVertex(secondDown, c1 * _alphaValue, new Vector2((float)Math.Sin(_counter / 20f), (float)Math.Sin(_counter / 20f)));
                }
                else
                {
                    if (i != _points.Count - 1)
                    {
                        Vector2 normal = CurveNormal(_points, i);
                        Vector2 normalAhead = CurveNormal(_points, i + 1);
                        float j = (_cap + ((float)(Math.Sin(_counter / 10f)) * 1) - i * 0.1f) / _cap;
                        widthVar *= j;
                        Vector2 firstUp = _points[i] - normal * widthVar;
                        Vector2 firstDown = _points[i] + normal * widthVar;
                        Vector2 secondUp = _points[i + 1] - normalAhead * widthVar;
                        Vector2 secondDown = _points[i + 1] + normalAhead * widthVar;

                        AddVertex(firstDown, c1 * _alphaValue, new Vector2((i / _cap), 1));
                        AddVertex(firstUp, c1 * _alphaValue, new Vector2((i / _cap), 0));
                        AddVertex(secondDown, c1 * _alphaValue, new Vector2((i + 1) / _cap, 1));

                        AddVertex(secondUp, c1 * _alphaValue, new Vector2((i + 1) / _cap, 0));
                        AddVertex(secondDown, c1 * _alphaValue, new Vector2((i + 1) / _cap, 1));
                        AddVertex(firstUp, c1 * _alphaValue, new Vector2((i / _cap), 0));
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}