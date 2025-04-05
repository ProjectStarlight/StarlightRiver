using ReLogic.Threading;
using StarlightRiver.Content.Configs;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.ScreenTargetSystem;

namespace StarlightRiver.Core.Systems.LightingSystem
{
	public class LightingBuffer : ILoadable
	{
		const int PADDING = 20;

		public static bool GettingColors = false;
		public static bool bufferNeedsPopulated = false;

		public static VertexBuffer lightingQuadBuffer;

		public static ScreenTarget screenLightingTarget;
		public static ScreenTarget tileLightingTarget;
		public static ScreenTarget tileLightingTempTarget;

		public static Vector2 tileLightingCenter;

		private static int refreshTimer;

		private static Color[] tileLightingBuffer;

		static float Factor => Main.screenHeight / (float)Main.screenWidth;

		static int XMax => Main.screenWidth / 16 + PADDING * 2;
		static int YMax => (int)(Main.screenHeight / 16 + PADDING * 2 * Factor);

		private static GraphicsConfig Config => ModContent.GetInstance<GraphicsConfig>();

		public void Load(Mod mod)
		{
			screenLightingTarget = new(DrawFinalTarget, () => bufferNeedsPopulated, 0.2f);
			tileLightingTarget = new(null, () => bufferNeedsPopulated, 0.1f, ResizeTile);
			tileLightingTempTarget = new(null, () => bufferNeedsPopulated, 0, ResizeTileTemp);
		}

		public void Unload()
		{

		}

		private static void SetupLightingQuadBuffer()
		{
			lightingQuadBuffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);

			var verticies = new VertexPositionColorTexture[6];

			verticies[0] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
			verticies[1] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));
			verticies[2] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

			verticies[3] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
			verticies[4] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
			verticies[5] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

			lightingQuadBuffer.SetData(verticies);
		}

		private static Vector2? ResizeTile(Vector2 input)
		{
			float factor = input.Y / input.X;
			return new Vector2(input.X / 16 + PADDING * 2, (int)(input.Y / 16 + PADDING * 2 * factor));
		}

		private static Vector2? ResizeTileTemp(Vector2 input)
		{
			return new Vector2(XMax, YMax);
		}

		private static void PopulateTileTexture(Vector2 start)
		{
			GettingColors = true;

			if (tileLightingBuffer is null || tileLightingBuffer.Length != tileLightingTarget.RenderTarget.Width * tileLightingTarget.RenderTarget.Height)
				tileLightingBuffer = new Color[tileLightingTarget.RenderTarget.Width * tileLightingTarget.RenderTarget.Height];

			int xTile = (int)start.X / 16;
			int yTile = (int)start.Y / 16;

			FastParallel.For(0, tileLightingTarget.RenderTarget.Width * tileLightingTarget.RenderTarget.Height, (from, to, context) =>
			{
				for (int k = from; k < to; k++)
				{
					int x = k % tileLightingTarget.RenderTarget.Width;
					int y = k / tileLightingTarget.RenderTarget.Width;
					tileLightingBuffer[k] = Lighting.GetColor(xTile + x, yTile + y);
				}
			});

			tileLightingTarget.RenderTarget.SetData(tileLightingBuffer);
			tileLightingCenter = start;
			GettingColors = false;
		}

		public static void PopulateScreenTexture()
		{
			GraphicsDevice graphics = Main.instance.GraphicsDevice;
			graphics.Clear(Color.Black);
			RenderLightingQuad();
		}

		private static void RenderLightingQuad()
		{
			if (Main.dedServ)
				return;

			// Added safety guard here since this seems to be able to be inconsistent?
			if (tileLightingTarget.RenderTarget is null)
				return;

			if (lightingQuadBuffer == null)
				SetupLightingQuadBuffer(); //a bit hacky, but if we do this on load we can end up with black textures for full screen users, and full screen does not fire set display mode events

			Effect upscaleEffect = ShaderLoader.GetShader("LightShader").Value;

			if (upscaleEffect is null)
				return;

			GraphicsDevice graphics = Main.instance.GraphicsDevice;

			graphics.SetVertexBuffer(lightingQuadBuffer);
			graphics.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

			Vector2 offset = (Main.screenPosition - tileLightingCenter) / new Vector2(Main.screenWidth, Main.screenHeight);

			upscaleEffect.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			upscaleEffect.Parameters["fullBufferSize"].SetValue(tileLightingTarget.RenderTarget.Size() * 16);
			upscaleEffect.Parameters["offset"].SetValue(offset);
			upscaleEffect.Parameters["sampleTexture"].SetValue(tileLightingTarget.RenderTarget);
			upscaleEffect.Parameters["transform"].SetValue(Main.LocalPlayer.gravDir != 1f ? new(1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1) : Matrix.Identity);
			Matrix a = Main.GameViewMatrix.TransformationMatrix;
			int b = 0;

			foreach (EffectPass pass in upscaleEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
			}
		}

		public static void DrawFinalTarget(SpriteBatch sb)
		{
			if (!bufferNeedsPopulated)
				return;

			refreshTimer++;

			if (Config.LightingPollRate != 0 && refreshTimer % Config.LightingPollRate == 0)
				PopulateTileTexture((Main.screenPosition / 16).Round() * 16 - Vector2.One * PADDING * 16);

			PopulateScreenTexture();

			bufferNeedsPopulated = false;
		}
	}

	public static class LightingBufferRenderer
	{
		private static readonly VertexPositionTexture[] verticies = new VertexPositionTexture[6];

		private static readonly VertexBuffer buffer = new(Main.instance.GraphicsDevice, typeof(VertexPositionTexture), 6, BufferUsage.WriteOnly);

		public static void DrawWithLighting(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale)
		{
			LightingBuffer.bufferNeedsPopulated = true;

			if (Main.dedServ || !ScreenTracker.OnScreenScreenspace(new Rectangle(destinationRectangle.X, destinationRectangle.Y, texture.Width, texture.Height)))
				return;

			Rectangle sourceToUse = sourceRectangle ?? texture.Bounds;

			destinationRectangle.Width = (int)(destinationRectangle.Width * scale.X);
			destinationRectangle.Height = (int)(destinationRectangle.Height * scale.Y);
			destinationRectangle.X -= (int)(origin.X * scale.X);
			destinationRectangle.Y -= (int)(origin.Y * scale.Y);

			Vector2 screenOrigin = destinationRectangle.TopLeft() + origin * scale;

			var zoom = //Main.GameViewMatrix.TransformationMatrix;
			new Matrix
			(
				Main.GameViewMatrix.TransformationMatrix.M11, 0, 0, 0,
				0, Main.GameViewMatrix.TransformationMatrix.M22, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1
			);

			Effect ApplyEffect = ShaderLoader.GetShader("LightApply").Value;

			if (ApplyEffect != null)
			{
				ApplyEffect.Parameters["zoom"].SetValue(zoom);
				ApplyEffect.Parameters["drawColor"].SetValue(color.ToVector4());

				ApplyEffect.Parameters["targetTexture"].SetValue(texture);
				ApplyEffect.Parameters["sampleTexture"].SetValue(LightingBuffer.screenLightingTarget.RenderTarget);

				ApplyEffect.Parameters["sampleTrans"].SetValue(Matrix.CreateScale(0.5f * 1 / Main.GameViewMatrix.TransformationMatrix.M11, -0.5f * 1 / Main.GameViewMatrix.TransformationMatrix.M11, 1f) * Matrix.CreateTranslation(0.5f, 0.5f, 0));

				verticies[0] = new VertexPositionTexture(new Vector3(destinationRectangle.TopLeft().RotatedBy(rotation, screenOrigin), 0).ToScreenspaceCoord(), sourceToUse.TopLeft() / texture.Size());
				verticies[1] = new VertexPositionTexture(new Vector3(destinationRectangle.TopRight().RotatedBy(rotation, screenOrigin), 0).ToScreenspaceCoord(), sourceToUse.TopRight() / texture.Size());
				verticies[2] = new VertexPositionTexture(new Vector3(destinationRectangle.BottomLeft().RotatedBy(rotation, screenOrigin), 0).ToScreenspaceCoord(), sourceToUse.BottomLeft() / texture.Size());

				verticies[3] = new VertexPositionTexture(new Vector3(destinationRectangle.TopRight().RotatedBy(rotation, screenOrigin), 0).ToScreenspaceCoord(), sourceToUse.TopRight() / texture.Size());
				verticies[4] = new VertexPositionTexture(new Vector3(destinationRectangle.BottomRight().RotatedBy(rotation, screenOrigin), 0).ToScreenspaceCoord(), sourceToUse.BottomRight() / texture.Size());
				verticies[5] = new VertexPositionTexture(new Vector3(destinationRectangle.BottomLeft().RotatedBy(rotation, screenOrigin), 0).ToScreenspaceCoord(), sourceToUse.BottomLeft() / texture.Size());

				buffer.SetData(verticies);

				Main.instance.GraphicsDevice.SetVertexBuffer(buffer);

				Main.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				Main.instance.GraphicsDevice.RasterizerState = Main.Rasterizer;

				foreach (EffectPass pass in ApplyEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
				}

				Main.instance.GraphicsDevice.SetVertexBuffer(null);
			}
		}

		public static void DrawWithLighting(Texture2D texture, Vector2 position, Color color)
		{
			DrawWithLighting(texture, new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height), texture.Bounds, color, 0, Vector2.Zero, Vector2.One);
		}

		public static void DrawWithLighting(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			DrawWithLighting(texture, new Rectangle((int)position.X, (int)position.Y, sourceRectangle?.Width ?? texture.Width, sourceRectangle?.Height ?? texture.Height), sourceRectangle, color, 0, Vector2.Zero, Vector2.One);
		}

		public static void DrawWithLighting(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale)
		{
			DrawWithLighting(texture, new Rectangle((int)position.X, (int)position.Y, sourceRectangle?.Width ?? texture.Width, sourceRectangle?.Height ?? texture.Height), sourceRectangle, color, rotation, origin, Vector2.One * scale);
		}

		public static void DrawWithLighting(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale)
		{
			DrawWithLighting(texture, new Rectangle((int)position.X, (int)position.Y, sourceRectangle?.Width ?? texture.Width, sourceRectangle?.Height ?? texture.Height), sourceRectangle, color, rotation, origin, scale);
		}

		public static void DrawWithLighting(Texture2D texture, Rectangle destinationRectangle, Color color)
		{
			DrawWithLighting(texture, destinationRectangle, texture.Bounds, color, 0, Vector2.Zero, Vector2.One);
		}

		public static void DrawWithLighting(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			DrawWithLighting(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, Vector2.One);
		}
	}
}