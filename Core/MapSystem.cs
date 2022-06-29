using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Configs;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
    public delegate void MapRender(SpriteBatch spriteBatch);

    public class Map
    {
        internal Dictionary<string, MapPass> MapPasses = new Dictionary<string, MapPass>();

        public void OrderedRenderPassBatched(SpriteBatch sb, GraphicsDevice GD, bool Batched = true)
        {
            RenderTargetBinding[] oldtargets1 = Main.graphics.GraphicsDevice.GetRenderTargets();
            int i = 0;

            Matrix matrix = Main.GameViewMatrix.ZoomMatrix;

            for (int a = 0; a < MapPasses.Count; a++)
            {
                foreach (KeyValuePair<string, MapPass> Map in MapPasses)
                {
                    var Pass = Map.Value;

                    if (Pass.Priority != i) continue;

                    if (Pass.ManualTarget == null)
                    {
                        GD.SetRenderTarget(Pass.MapTarget);
                        GD.Clear(Color.Transparent);

                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                        Pass.RenderBatched(sb);
                        sb.End();

                        Pass.RenderPrimitive(sb);
                    }
                }

                i++;
            }

            Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets1);
        }

        public void OrderedShaderPass()
        {
            int i = 0;

            for (int a = 0; a < MapPasses.Count; a++)
            {
                foreach (KeyValuePair<string, MapPass> Map in MapPasses)
                {
                    var Pass = Map.Value;

                    if (Pass.Priority != i) continue;

                    Pass.ApplyShader();
                }

                i++;
            }
        }
        public void DrawToMap(string Map, MapRender MR) => MapPasses[Map].DrawToBatchedTarget(MR);

        public void AddMap(string MapName, MapPass MP)
        {
            MP.Parent = this;
            MapPasses.Add(MapName, MP);
        }

        public MapPass Get(string MapName) => MapPasses[MapName];

        public MapPass Get<T>() where T : MapPass
        {
            //TODO: Support for multiple Passes with different ID's

            foreach (MapPass pass in MapPasses.Values)
            {
                if (pass is T) return (T)pass;
            }

            throw new System.Exception("Pass does not exist");
        }

    }

    public abstract class MapPass
    {
        internal event MapRender BatchedCalls;

        internal event MapRender PrimitiveCalls;

        public RenderTarget2D MapTarget;

        public virtual RenderTarget2D ManualTarget => null;

        public abstract int Priority { get; }

        protected abstract string MapEffectName { get; }

        protected ScreenShaderData MapEffect => Helpers.Helpers.GetScreenShader(MapEffectName);

        internal virtual void OnApplyShader() { }
        public virtual void Load() => MapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);


        public void ApplyShader()
        {
            if (ManualTarget != null) MapTarget = ManualTarget;

            if (MapEffectName != "")
            {
                //always here jic
                MapEffect?.Shader?.Parameters["Noise"]?.SetValue(Request<Texture2D>(AssetDirectory.Assets + "Noise/ShaderNoise").Value);
                MapEffect?.Shader?.Parameters["TileTarget"]?.SetValue(PlayerTarget.ScaledTileTarget);
                MapEffect?.Shader?.Parameters["Map"]?.SetValue(MapTarget);

                //change to something better
                MapEffect?.UseIntensity(Main.GameUpdateCount);
            }

            OnApplyShader();

            if (MapEffectName != "")
            {
                Helpers.Helpers.ActivateScreenShader(MapEffectName);
            }

        }

        public void DrawToBatchedTarget(MapRender method) => BatchedCalls += method;

        public void DrawToPrimitiveTarget(MapRender method) => PrimitiveCalls += method;

        public void RenderBatched(SpriteBatch spriteBatch)
        {
            BatchedCalls?.Invoke(spriteBatch);
            BatchedCalls = null;
        }

        public void RenderPrimitive(SpriteBatch spriteBatch)
        {
            PrimitiveCalls?.Invoke(spriteBatch);
            PrimitiveCalls = null;
        }

        public Map Parent;
        public MapPass() => Load();

    }
}

