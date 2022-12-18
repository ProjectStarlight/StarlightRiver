using StarlightRiver.Content.CustomHooks;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public delegate void MapRender(SpriteBatch spriteBatch);

	public class Map
	{
		internal Dictionary<string, MapPass> MapPasses = new();

		public void OrderedRenderPassBatched(SpriteBatch sb, GraphicsDevice graphics)
		{
			RenderTargetBinding[] oldtargets1 = Main.graphics.GraphicsDevice.GetRenderTargets();
			int i = 0;

			Matrix matrix = Main.GameViewMatrix.ZoomMatrix;

			for (int a = 0; a < MapPasses.Count; a++)
			{
				foreach (KeyValuePair<string, MapPass> Map in MapPasses)
				{
					MapPass Pass = Map.Value;

					if (Pass.Priority != i)
						continue;

					if (Pass.ManualTarget == null)
					{
						graphics.SetRenderTarget(Pass.mapTarget);
						graphics.Clear(Color.Transparent);

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
					MapPass Pass = Map.Value;

					if (Pass.Priority != i)
						continue;

					Pass.ApplyShader();
				}

				i++;
			}
		}

		public void DrawToMap(string Map, MapRender MR)
		{
			MapPasses[Map].DrawToBatchedTarget(MR);
		}

		public void AddMap(string MapName, MapPass MP)
		{
			MP.Parent = this;
			MapPasses.Add(MapName, MP);
		}

		public MapPass Get(string MapName)
		{
			return MapPasses[MapName];
		}

		public MapPass Get<T>() where T : MapPass
		{
			//TODO: Support for multiple Passes with different ID's

			foreach (MapPass pass in MapPasses.Values)
			{
				if (pass is T)
					return (T)pass;
			}

			throw new System.Exception("Pass does not exist");
		}
	}

	public abstract class MapPass
	{
		internal event MapRender batchedCalls;

		internal event MapRender primitiveCalls;

		public RenderTarget2D mapTarget;

		public abstract int Priority { get; }

		protected abstract string MapEffectName { get; }

		public virtual RenderTarget2D ManualTarget => null;

		protected ScreenShaderData MapEffect => Helpers.Helpers.GetScreenShader(MapEffectName);

		internal virtual void OnApplyShader() { }

		public virtual void Load()
		{
			mapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
		}

		public void ApplyShader()
		{
			if (ManualTarget != null)
				mapTarget = ManualTarget;

			if (MapEffectName != "")
			{
				//always here jic
				MapEffect?.Shader?.Parameters["Noise"]?.SetValue(Request<Texture2D>(AssetDirectory.Assets + "Noise/ShaderNoise").Value);
				MapEffect?.Shader?.Parameters["TileTarget"]?.SetValue(PlayerTarget.ScaledTileTarget);
				MapEffect?.Shader?.Parameters["Map"]?.SetValue(mapTarget);

				//change to something better
				MapEffect?.UseIntensity(Main.GameUpdateCount);
			}

			OnApplyShader();

			if (MapEffectName != "")
			{
				Helpers.Helpers.ActivateScreenShader(MapEffectName);
			}
		}

		public void DrawToBatchedTarget(MapRender method)
		{
			batchedCalls += method;
		}

		public void DrawToPrimitiveTarget(MapRender method)
		{
			primitiveCalls += method;
		}

		public void RenderBatched(SpriteBatch spriteBatch)
		{
			batchedCalls?.Invoke(spriteBatch);
			batchedCalls = null;
		}

		public void RenderPrimitive(SpriteBatch spriteBatch)
		{
			primitiveCalls?.Invoke(spriteBatch);
			primitiveCalls = null;
		}

		public Map Parent;
		public MapPass()
		{
			Load();
		}
	}
}

