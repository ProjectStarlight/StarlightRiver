using StarlightRiver.Content.Configs;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using StarlightRiver.Helpers;
using Terraria.Graphics.Effects;
using static StarlightRiver.Helpers.DrawHelper;

namespace StarlightRiver.Core.Systems.LightingSystem
{
	public class LightingBuffer
	{
		const int PADDING = 20;

		public static bool GettingColors = false;

		public static VertexBuffer lightingQuadBuffer;

		public static ScreenTarget screenLightingTarget = new(DrawFinalTarget, () => true, 0.2f);
		public static ScreenTarget tileLightingTarget = new(null, () => true, 0.1f, ResizeTile);
		public static ScreenTarget tileLightingTempTarget = new(null, () => true, 0, ResizeTileTemp);

		public static Vector2 tileLightingCenter;

		private static int refreshTimer;

		static float Factor => Main.screenHeight / (float)Main.screenWidth;

		static int XMax => Main.screenWidth / 16 + PADDING * 2;
		static int YMax => (int)(Main.screenHeight / 16 + PADDING * 2 * Factor);

		private static GraphicsConfig Config => ModContent.GetInstance<GraphicsConfig>();

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

			var tileLightingBuffer = new Color[tileLightingTarget.RenderTarget.Width * tileLightingTarget.RenderTarget.Height];

			for (int x = 0; x < tileLightingTarget.RenderTarget.Width; x++)
			{
				for (int y = 0; y < tileLightingTarget.RenderTarget.Height; y++)
				{
					int index = y * tileLightingTarget.RenderTarget.Width + x;

					if (tileLightingBuffer.Length > index)
						tileLightingBuffer[index] = Lighting.GetColor((int)start.X / 16 + x, (int)start.Y / 16 + y);
				}
			}

			tileLightingTarget.RenderTarget.SetData(tileLightingBuffer);
			tileLightingCenter = start;
			GettingColors = false;
		}

		private static void PopulateTileTextureScrolling(Vector2 start, int yToStart, int yToEnd)
		{
			GettingColors = true;
			var tileLightingBuffer = new Color[tileLightingTempTarget.RenderTarget.Width * (yToEnd - yToStart)];

			for (int x = 0; x < tileLightingTempTarget.RenderTarget.Width; x++)
			{
				for (int y = yToStart; y < yToEnd; y++)
				{
					int index = (y - yToStart) * tileLightingTempTarget.RenderTarget.Width + x;

					if (tileLightingBuffer.Length > index)
						tileLightingBuffer[index] = Lighting.GetColor((int)start.X / 16 + x, (int)start.Y / 16 + y);
				}
			}

			if (tileLightingBuffer is null || tileLightingBuffer.Length == 0)
				return;

			tileLightingTempTarget.RenderTarget.SetData(0, new Rectangle(0, yToStart, tileLightingTempTarget.RenderTarget.Width, yToEnd - yToStart), tileLightingBuffer, 0, tileLightingTempTarget.RenderTarget.Width * (yToEnd - yToStart));

			if (refreshTimer % Config.LightingPollRate == 0)
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

			if (lightingQuadBuffer == null)
				SetupLightingQuadBuffer(); //a bit hacky, but if we do this on load we can end up with black textures for full screen users, and full screen does not fire set display mode events

			Effect upscaleEffect = Filters.Scene["LightShader"].GetShader().Shader;

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

			foreach (EffectPass pass in upscaleEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
			}
		}

		public static void DrawFinalTarget(SpriteBatch sb)
		{
			if (ModContent.GetInstance<GraphicsConfig>().HighQualityLighting)
			{
				refreshTimer++;

				if (Config.ScrollingLightingPoll)
				{
					int progress = refreshTimer % Config.LightingPollRate;

					PopulateTileTextureScrolling((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * PADDING * 16,
						(int)(progress / (float)Config.LightingPollRate * tileLightingTarget.RenderTarget.Height),
						(int)((progress + 1) / (float)Config.LightingPollRate * tileLightingTarget.RenderTarget.Height));

					if (refreshTimer % Config.LightingPollRate == 0)
					{
						var colorData = new Color[tileLightingTarget.RenderTarget.Width * tileLightingTarget.RenderTarget.Height];
						tileLightingTempTarget.RenderTarget.GetData(colorData);
						tileLightingTarget.RenderTarget.SetData(colorData);
					}
				}
				else
				{
					if (Config.LightingPollRate != 0 && refreshTimer % Config.LightingPollRate == 0)
						PopulateTileTexture((Main.screenPosition / 16).ToPoint16().ToVector2() * 16 - Vector2.One * PADDING * 16);
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

		private static readonly VertexBuffer buffer = new(Main.instance.GraphicsDevice, typeof(VertexPositionTexture), 6, BufferUsage.WriteOnly);
		private static readonly VertexBuffer bufferColor = new(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 12, BufferUsage.WriteOnly);

		//Scale is important here instead of just modifying the pos rectangle to change where the texture samples from the lighting buffer, otherwise it would sample from the base points
		public static void DrawWithLighting(Rectangle pos, Texture2D tex, Rectangle source, Color color = default, Vector2 scale = default)
		{
			//TODO: Include an origin that the point scales from
			if (Main.dedServ || !Helper.OnScreen(new Rectangle(pos.X, pos.Y, tex.Width, tex.Height)))
				return;

			if (color == default)
				color = Color.White;

			if (scale == default)
				scale = Vector2.One;

			var zoom =  //Main.GameViewMatrix.TransformationMatrix;
			new Matrix
			(
				Main.GameViewMatrix.Zoom.X, 0, 0, 0,
				0, Main.GameViewMatrix.Zoom.X, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1
			);

			if (!ModContent.GetInstance<GraphicsConfig>().HighQualityLighting)
			{
				var scaledPos = new Rectangle((int)(pos.X * scale.X), (int)(pos.Y * scale.Y), (int)(pos.Width * scale.X), (int)(pos.Height * scale.Y));
				var checkZone = Rectangle.Intersect(scaledPos, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));
				Color topLeftColor = Lighting.GetColor((checkZone.X + (int)Main.screenPosition.X) / 16, (checkZone.Y + (int)Main.screenPosition.Y) / 16, color);
				Color topRightColor = Lighting.GetColor((checkZone.X + checkZone.Width + (int)Main.screenPosition.X) / 16, (checkZone.Y + (int)Main.screenPosition.Y) / 16, color);
				Color bottomLeftColor = Lighting.GetColor((checkZone.X + (int)Main.screenPosition.X) / 16, (checkZone.Y + checkZone.Height + (int)Main.screenPosition.Y) / 16, color);
				Color bottomRightColor = Lighting.GetColor((checkZone.X + checkZone.Width + (int)Main.screenPosition.X) / 16, (checkZone.Y + checkZone.Height + (int)Main.screenPosition.Y) / 16, color);
				Color centerColor = Lighting.GetColor((checkZone.Center.X + (int)Main.screenPosition.X) / 16, (checkZone.Center.Y + (int)Main.screenPosition.Y) / 16, color);

				verticiesColor[0] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopLeft() * scale), 0),
					topLeftColor, source.TopLeft() / tex.Size());
				verticiesColor[1] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopRight() * scale), 0),
					topRightColor, source.TopRight() / tex.Size());
				verticiesColor[2] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center() * scale), 0),
					centerColor, source.Center() / tex.Size());

				verticiesColor[3] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopRight() * scale), 0),
				   topRightColor, source.TopRight() / tex.Size());
				verticiesColor[4] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomRight() * scale), 0),
					bottomRightColor, source.BottomRight() / tex.Size());
				verticiesColor[5] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center() * scale), 0),
					centerColor, source.Center() / tex.Size());

				verticiesColor[6] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center() * scale), 0),
					centerColor, source.Center() / tex.Size());
				verticiesColor[7] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomRight() * scale), 0),
					bottomRightColor, source.BottomRight() / tex.Size());
				verticiesColor[8] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomLeft() * scale), 0),
					bottomLeftColor, source.BottomLeft() / tex.Size());

				verticiesColor[9] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.TopLeft() * scale), 0),
					topLeftColor, source.TopLeft() / tex.Size());
				verticiesColor[10] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.Center() * scale), 0),
					centerColor, source.Center() / tex.Size());
				verticiesColor[11] = new VertexPositionColorTexture(new Vector3(ConvertVec2(pos.BottomLeft() * scale), 0),
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
				ApplyEffect.Parameters["texSize"].SetValue(tex.Size() * scale);
				ApplyEffect.Parameters["offset"].SetValue((pos.TopLeft() - source.TopLeft()) / new Vector2(Main.screenWidth, Main.screenHeight));
				ApplyEffect.Parameters["zoom"].SetValue(zoom);
				ApplyEffect.Parameters["drawColor"].SetValue(color.ToVector4());

				ApplyEffect.Parameters["targetTexture"].SetValue(tex);
				ApplyEffect.Parameters["sampleTexture"].SetValue(LightingBuffer.screenLightingTarget.RenderTarget);

				verticies[0] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.TopLeft() * scale), 0), source.TopLeft() / tex.Size());
				verticies[1] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.TopRight() * scale), 0), source.TopRight() / tex.Size());
				verticies[2] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.BottomLeft() * scale), 0), source.BottomLeft() / tex.Size());

				verticies[3] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.TopRight() * scale), 0), source.TopRight() / tex.Size());
				verticies[4] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.BottomRight() * scale), 0), source.BottomRight() / tex.Size());
				verticies[5] = new VertexPositionTexture(new Vector3(ConvertVec2(pos.BottomLeft() * scale), 0), source.BottomLeft() / tex.Size());

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

		public static void DrawWithLighting(Vector2 pos, Texture2D tex, Rectangle source, Color color = default, Vector2 scale = default)
		{
			DrawWithLighting(new Rectangle((int)pos.X, (int)pos.Y, source.Width, source.Height), tex, source, color, scale);
		}

		public static void DrawWithLighting(Vector2 pos, Texture2D tex, Color color = default, Vector2 scale = default)
		{
			DrawWithLighting(pos, tex, tex.Frame(), color, scale);
		}
	}
}