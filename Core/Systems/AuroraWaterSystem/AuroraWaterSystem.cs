using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.NPCs.Permafrost;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.MetaballSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.AuroraWaterSystem

{
	struct AuroraWaterData : ITileData
	{
		public byte PackedData;

		public bool HasAuroraWater
		{
			get => (PackedData & 0b00000001) == 1;
			set => PackedData = (byte)(value ? PackedData | 0x01 : PackedData & ~0x01);
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

	class AuroraRipple
	{
		public Vector2 pos;
		public float scale;
		public float speed;
		public float prog;

		public AuroraRipple(Vector2 pos, float scale, float speed)
		{
			this.pos = pos;
			this.scale = scale;
			this.speed = speed;
		}	
	}

	class AuroraWaterSystem : ModSystem
	{
		public static int visCounter = 0;
		public static bool Visible => visCounter > 0 || Main.LocalPlayer.InModBiome(ModContent.GetInstance<PermafrostTempleBiome>());

		public static ScreenTarget auroraTarget;
		public static ScreenTarget auroraBackTarget;

		public static bool failedLoad = false;

		public static List<AuroraRipple> ripplePoints = new();

		public float Priority => 1;

		public override void Load()
		{
			On_Main.DrawInfernoRings += DrawAuroraWater;
			auroraTarget = new(DrawAuroraTarget, () => Visible, 1);
			auroraBackTarget = new(DrawAuroraBackTarget, () => Visible, 1);
		}

		public static void DrawToMetaballTarget()
		{
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
							var target = new Rectangle((int)(i * 16 - Main.screenPosition.X) / 2, (int)(j * 16 - Main.screenPosition.Y) / 2, 8, 8);
							Texture2D tex = Assets.Misc.AuroraWater.Value;
							Main.spriteBatch.Draw(tex, target, new Rectangle(tileData.AuroraWaterFrameX * 18, tileData.AuroraWaterFrameY * 18, 16, 16), Color.White);
						}
					}
				}
			}
		}

		private static void DrawAuroraTarget(SpriteBatch sb)
		{
			Asset<Texture2D> asset = Assets.Misc.AuroraWaterMap;

			if (asset.IsLoaded)
			{
				sb.End();
				sb.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

				Texture2D tex = asset.Value;

				for (int i = -tex.Width; i <= Main.screenWidth + tex.Width; i += tex.Width)
				{
					for (int j = -tex.Height; j <= Main.screenHeight + tex.Height; j += tex.Height)
					{
						Main.spriteBatch.Draw(tex, new Vector2(i, j),
							new Rectangle(
								(int)(Main.screenPosition.X % tex.Width - Main.GameUpdateCount * 0.55f),
								(int)(Main.screenPosition.Y % tex.Height + Main.GameUpdateCount * 0.3f),
								tex.Width,
								tex.Height
								),
							Color.White * 0.7f, default, default, 1, 0, 0);

						Main.spriteBatch.Draw(tex, new Vector2(i, j),
							new Rectangle(
								(int)(Main.screenPosition.X % tex.Width + Main.GameUpdateCount * 0.75f),
								(int)(Main.screenPosition.Y % tex.Height - Main.GameUpdateCount * 0.4f),
								tex.Width,
								tex.Height
								),
							Color.White);
					}
				}
			}
		}

		private static void DrawAuroraBackTarget(SpriteBatch sb)
		{
			Asset<Texture2D> asset = Assets.Misc.AuroraWaterMap;
			Asset<Texture2D> asset2 = Assets.Noise.SwirlyNoiseLooping;

			if (asset.IsLoaded)
			{
				sb.End();
				sb.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, RasterizerState.CullNone, default);

				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				Texture2D tex = asset.Value;
				Texture2D tex2 = asset2.Value;
				Texture2D tex3 = Assets.Masks.Glow.Value;
				Texture2D rippleTex = Assets.Masks.RingGlowInnerTwo.Value;

				Vector2 layer1Pivot = Main.GameUpdateCount * new Vector2(-0.55f, 0.3f);
				Vector2 layer2Pivot = Main.GameUpdateCount * new Vector2(0.75f, -0.4f);
				sb.Draw(tex,
					Vector2.Zero,
					new Rectangle(
						(int)(Main.screenPosition.X + layer1Pivot.X) % tex.Width,
						(int)(Main.screenPosition.Y + layer1Pivot.Y) % tex.Height,
						Main.screenWidth,
						Main.screenHeight),
					Color.Red * 0.7f);
				sb.Draw(tex,
					Vector2.Zero,
					new Rectangle(
						(int)(Main.screenPosition.X + layer2Pivot.X) % tex.Width,
						(int)(Main.screenPosition.Y + layer2Pivot.Y) % tex.Height,
						Main.screenWidth,
						Main.screenHeight),
					Color.Red);

				sb.Draw(tex2,
					Vector2.Zero,
					new Rectangle(
						(int)(Main.screenPosition.X + layer1Pivot.X) % tex2.Width,
						(int)(Main.screenPosition.Y + layer1Pivot.Y) % tex2.Height,
						Main.screenWidth,
						Main.screenHeight),
					Color.Green * 0.7f);
				sb.Draw(tex2,
					Vector2.Zero,
					new Rectangle(
						(int)(Main.screenPosition.X + layer2Pivot.X) % tex2.Width,
						(int)(Main.screenPosition.Y + layer2Pivot.Y) % tex2.Height,
						Main.screenWidth,
						Main.screenHeight),
					Color.Green);

				sb.Draw(tex3,
					Main.LocalPlayer.Center - Main.screenPosition,
					null,
					Color.Blue, 0, tex3.Size() / 2f, 3f, 0, 0);

				foreach(var ripple in ripplePoints)
				{
					var col = Color.Lime * (1f - ripple.prog) * ripple.scale;
					var col2 = Color.Red * (1f - ripple.prog) * ripple.scale;
					sb.Draw(rippleTex, ripple.pos - Main.screenPosition, null, col, 0, rippleTex.Size() / 2f, ripple.scale * ripple.prog, 0, 0);
					sb.Draw(rippleTex, ripple.pos - Main.screenPosition, null, col2, 0, rippleTex.Size() / 2f, ripple.scale * ripple.prog, 0, 0);
				}

				sb.End();
				sb.Begin();
			}
		}

		public override void PostUpdateEverything()
		{
			for(int k = 0; k < ripplePoints.Count; k++)
			{
				ripplePoints[k].prog += ripplePoints[k].speed;
			}

			ripplePoints.RemoveAll(n => n.prog >= 1f);
		}

		public static void AddRipple(Vector2 pos, float scale, float speed)
		{
			ripplePoints.Add(new(pos, scale, speed));
		}

		private void DrawAuroraWater(On_Main.orig_DrawInfernoRings orig, Main self)
		{
			orig(self);

			if (Visible)
			{
				AuroraWaterTileMetaballs.DrawSpecial();
				visCounter--;
			}
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

		public override unsafe void SaveWorldData(TagCompound tag)
		{
			if (failedLoad)
			{
				Mod.Logger.Info("Did not save aurora water data as it previously failed to load.");
				failedLoad = false;
				return;
			}

			AuroraWaterData[] myData = Main.tile.GetData<AuroraWaterData>();
			byte[] data = new byte[myData.Length];

			fixed (AuroraWaterData* ptr = myData)
			{
				byte* bytePtr = (byte*)ptr;
				var span = new Span<byte>(bytePtr, myData.Length);
				var target = new Span<byte>(data);
				span.CopyTo(target);
			}

			tag.Add("tileData", data);
		}

		public override unsafe void LoadWorldData(TagCompound tag)
		{
			AuroraWaterData[] targetData = Main.tile.GetData<AuroraWaterData>();
			byte[] data = tag.GetByteArray("tileData");

			if (targetData.Length != data.Length)
			{
				Mod.Logger.Error($"Failed to load aurora water raw data, saved data was of incorrect size. Loaded data was {data.Length}, expected {targetData.Length}. Aurora water will not be loaded.");
				failedLoad = true;
				return;
			}

			fixed (AuroraWaterData* ptr = targetData)
			{
				byte* bytePtr = (byte*)ptr;
				var span = new Span<byte>(bytePtr, targetData.Length);
				var target = new Span<byte>(data);
				target.CopyTo(span);
			}
		}
	}

	class AuroraWaterGlobalTile : GlobalTile
	{
		public override void NearbyEffects(int i, int j, int type, bool closer)
		{
			if (Main.tile[i, j].Get<AuroraWaterData>().HasAuroraWater)
			{
				AuroraWaterSystem.visCounter = 30;

				if (i % 2 == 0 && j % 2 == 0 && !Main.tile[i, j].IsSquareSolidTile())
					Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(0.4f, 0.8f, 1f));
			}
		}
	}

	class AuroraWaterTileMetaballs : MetaballActor
	{
		public override bool Active => AuroraWaterSystem.Visible && !Main.LocalPlayer.InModBiome(ModContent.GetInstance<Content.Biomes.PermafrostTempleBiome>());

		public override Color OutlineColor => new(255, 0, 0);

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			AuroraWaterSystem.DrawToMetaballTarget();

			Texture2D tex = Assets.Items.Misc.MagmaGunProj.Value;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

			foreach (NPC npc in Main.npc)
			{
				if (npc.active && npc.ModNPC is WaterCube)
					(npc.ModNPC as WaterCube).DrawToTarget(spriteBatch);
			}

			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == ModContent.DustType<AuroraWaterFast>())
					spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, Color.Lime, 0f, Vector2.One * 256f, dust.scale * 0.05f, SpriteEffects.None, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}

		public static void DrawSpecial()
		{
			MetaballSystem.MetaballSystem.actorsSem.WaitOne();

			if (!MetaballSystem.MetaballSystem.actors.FirstOrDefault(n => n is AuroraWaterTileMetaballs).Active)
			{
				MetaballSystem.MetaballSystem.actorsSem.Release();
				return;
			}

			Effect effect = ShaderLoader.GetShader("AuroraWaterShader").Value;

			if (effect is null)
			{
				MetaballSystem.MetaballSystem.actorsSem.Release();
				return;
			}

			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTargetSwap);

			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
			effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth * -0.5f, Main.screenPosition.Y / Main.screenHeight * -0.5f));
			effect.Parameters["sampleTexture"].SetValue(AuroraWaterSystem.auroraBackTarget.RenderTarget);
			effect.Parameters["uImageSize1"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			//effect.Parameters["lightTexture"].SetValue(LightingBuffer.screenLightingTarget.RenderTarget);
			effect.Parameters["gameTexture"].SetValue(Main.screenTarget);
			effect.Parameters["transform"].SetValue(Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));
			effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X % Main.screenWidth / Main.screenWidth, Main.screenPosition.Y % Main.screenHeight / Main.screenHeight));

			var inv = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, effect, Matrix.Identity);

			Texture2D target = MetaballSystem.MetaballSystem.actors.FirstOrDefault(n => n is AuroraWaterTileMetaballs).Target.RenderTarget;
			Main.spriteBatch.Draw(target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2, 0, 0);

			Main.spriteBatch.End();

			Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTarget);

			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Matrix.Identity);
			Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, 0, 0);

			MetaballSystem.MetaballSystem.actorsSem.Release();
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			return false;
		}
	}
}