using StarlightRiver.Core.Systems.ScreenTargetSystem;

namespace StarlightRiver.Core.Systems.AuroraWaterSystem
{
	struct AuroraWaterData : ITileData
	{
		public byte PackedData;

		public bool HasAuroraWater
		{
			get => (PackedData & 0b00000001) == 1;
			set => PackedData = (byte)(~PackedData & (value ? 1 : 0));
		}

		public int AuroraWaterFrameX
		{
			get => (PackedData & 0b00001110) >> 1;
			set => PackedData = (byte)(PackedData & 0b11110001 | value << 1);
		}

		public int AuroraWaterFrameY
		{
			get => (PackedData & 0b01110000) >> 4;
			set => PackedData = (byte)(PackedData & 0b10001111 | value << 4);
		}
	}

	class AuroraWaterSystem : ModSystem, IOrderedLoadable
	{
		public static ScreenTarget auroraTarget = new(DrawAuroraTarget, () => !Main.dedServ && !Main.gameMenu, 1);
		public static ScreenTarget auroraBackTarget = new(DrawAuroraBackTarget, () => !Main.dedServ && !Main.gameMenu, 1);

		public float Priority => 1;

		private static void DrawAuroraTarget(SpriteBatch sb)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			for (int i = -2 + (int)Main.screenPosition.X / 16; i <= 2 + (int)(Main.screenPosition.X + Main.screenWidth) / 16; i++)
			{
				for (int j = -2 + (int)Main.screenPosition.Y / 16; j <= 2 + (int)(Main.screenPosition.Y + Main.screenHeight) / 16; j++)
				{
					if (WorldGen.InWorld(i, j))
					{
						Tile tile = Framing.GetTileSafely(i, j);
						ref AuroraWaterData tileData = ref tile.Get<AuroraWaterData>();

						if (tileData.HasAuroraWater)
						{
							var target = new Rectangle((int)(i * 16 - Main.screenPosition.X), (int)(j * 16 - Main.screenPosition.Y), 16, 16);
							Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Misc/AuroraWater").Value;
							sb.Draw(tex, target, new Rectangle(tileData.AuroraWaterFrameX * 18, tileData.AuroraWaterFrameY * 18, 16, 16), Color.White * 0.5f);
						}
					}
				}
			}
		}

		private static void DrawAuroraBackTarget(SpriteBatch sb)
		{
			sb.End();
			sb.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/AuroraWaterMap", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

			for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
			{
				for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
				{
					sb.Draw(tex2, new Vector2(i, j),
						new Rectangle(
							(int)(Main.screenPosition.X % tex2.Width - Main.GameUpdateCount * 0.55f),
							(int)(Main.screenPosition.Y % tex2.Height + Main.GameUpdateCount * 0.3f),
							tex2.Width,
							tex2.Height
							),
						Color.White * 0.7f, default, default, 1, 0, 0);

					sb.Draw(tex2, new Vector2(i, j),
						new Rectangle(
							(int)(Main.screenPosition.X % tex2.Width + Main.GameUpdateCount * 0.75f),
							(int)(Main.screenPosition.Y % tex2.Height - Main.GameUpdateCount * 0.4f),
							tex2.Width,
							tex2.Height
							),
						Color.White);
				}
			}

			sb.End();
			sb.Begin();
		}

		public override void PostDrawTiles()
		{
			Effect shader = Terraria.Graphics.Effects.Filters.Scene["AuroraWaterShader"].GetShader().Shader;

			if (shader is null || auroraTarget.RenderTarget is null)
				return;

			shader.Parameters["time"].SetValue(StarlightWorld.visualTimer);
			shader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			shader.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X % Main.screenWidth / Main.screenWidth, Main.screenPosition.Y % Main.screenHeight / Main.screenHeight));
			shader.Parameters["sampleTexture2"].SetValue(auroraBackTarget.RenderTarget);

			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, shader);

			Main.spriteBatch.Draw(auroraTarget.RenderTarget, Vector2.Zero, Color.White);

			Main.spriteBatch.End();
		}

		public static void PlaceAuroraWater(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			ref AuroraWaterData tileData = ref tile.Get<AuroraWaterData>();

			tileData.HasAuroraWater = true;
			FrameAuroraTile(i, j);

			FrameAuroraTile(i - 1, j);
			FrameAuroraTile(i, j - 1);
			FrameAuroraTile(i + 1, j);
			FrameAuroraTile(i, j + 1);
		}

		public static void RemoveAuroraWater(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			ref AuroraWaterData tileData = ref tile.Get<AuroraWaterData>();

			tileData.HasAuroraWater = false;
			tileData.AuroraWaterFrameX = 0;
			tileData.AuroraWaterFrameY = 0;

			FrameAuroraTile(i - 1, j);
			FrameAuroraTile(i, j - 1);
			FrameAuroraTile(i + 1, j);
			FrameAuroraTile(i, j + 1);
		}

		private static void FrameAuroraTile(int i, int j)
		{
			ref AuroraWaterData data = ref Framing.GetTileSafely(i, j).Get<AuroraWaterData>();

			if (!data.HasAuroraWater)
				return;

			bool u = Framing.GetTileSafely(i, j - 1).Get<AuroraWaterData>().HasAuroraWater;
			bool l = Framing.GetTileSafely(i - 1, j).Get<AuroraWaterData>().HasAuroraWater;
			bool d = Framing.GetTileSafely(i, j + 1).Get<AuroraWaterData>().HasAuroraWater;
			bool r = Framing.GetTileSafely(i + 1, j).Get<AuroraWaterData>().HasAuroraWater;

			if (u && l && d && r)
				SetFrameData(ref data, 3, 2);
			else if (l && r && d)
				SetFrameData(ref data, 1, 0);
			else if (u && d && r)
				SetFrameData(ref data, 1, 1);
			else if (l && r && u)
				SetFrameData(ref data, 1, 2);
			else if (u && d && l)
				SetFrameData(ref data, 1, 3);
			else if (r && d)
				SetFrameData(ref data, 0, 0);
			else if (u && r)
				SetFrameData(ref data, 0, 1);
			else if (l && u)
				SetFrameData(ref data, 0, 2);
			else if (d && l)
				SetFrameData(ref data, 0, 3);
			else if (u && d)
				SetFrameData(ref data, 3, 0);
			else if (l && r)
				SetFrameData(ref data, 3, 1);
			else if (u)
				SetFrameData(ref data, 2, 0);
			else if (l)
				SetFrameData(ref data, 2, 1);
			else if (d)
				SetFrameData(ref data, 2, 2);
			else if (r)
				SetFrameData(ref data, 2, 3);
			else
				SetFrameData(ref data, 3, 3);
		}

		private static void SetFrameData(ref AuroraWaterData data, int x, int y)
		{
			data.AuroraWaterFrameX = x;
			data.AuroraWaterFrameY = y;
		}
	}
}
