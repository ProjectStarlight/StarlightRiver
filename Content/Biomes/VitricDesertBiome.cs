using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Waters;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Biomes
{
	public class VitricDesertBiome : ModBiome
	{
		public static bool onScreen;

		internal static ParticleSystem ForegroundParticles;
		internal static ParticleSystem BackgroundParticles;

		internal static Asset<Texture2D>[] textures =
		{
			Assets.Backgrounds.Glass0,
			Assets.Backgrounds.Glass1,
			Assets.Backgrounds.Glass2,
			Assets.Backgrounds.Glass3,
			Assets.Backgrounds.Glass4,
			Assets.Backgrounds.Glass5
		};

		private static Vector2 parallaxOrigin;
		private static float vanillaParallax;

		public override string BestiaryIcon => AssetDirectory.Biomes + "VitricDesertIcon";

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/GlassPassive");

		public override string MapBackground => AssetDirectory.MapBackgrounds + "GlassMap";

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override ModWaterStyle WaterStyle => ModContent.GetInstance<WaterVitric>();

		public override void Load()
		{
			if (Main.dedServ)
				return;

			ForegroundParticles = new ParticleSystem("StarlightRiver/Assets/GUI/HolyBig", UpdateForegroundBody, ParticleSystem.AnchorOptions.World);
			BackgroundParticles = new ParticleSystem("StarlightRiver/Assets/GUI/Holy", UpdateBackgroundBody, ParticleSystem.AnchorOptions.World);

			if (ModLoader.TryGetMod("NotQuiteNitrate", out Mod nqn))
				nqn.Call("ModifyDrawBlackThreshold", (Func<float, float>)NewThreshold);

			On_Main.DrawBackgroundBlackFill += DrawVitricBackground;
			On_Main.DrawBlack += ForceDrawBlack;
			IL_Main.DrawBlack += ChangeBlackThreshold;

			Filters.Scene["GradientDistortion"] = new Filter(new ScreenShaderData(ShaderLoader.GetShader("GradientDistortion"), "GradientDistortion" + "Pass"), EffectPriority.High);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Desert");
		}

		public override bool IsBiomeActive(Player player)
		{
			if (StarlightWorld.vitricBiome == default)
				return false;

			Rectangle detectionBox = StarlightWorld.vitricBiome;
			detectionBox.Inflate(Main.screenWidth / 32, Main.screenHeight / 32);

			onScreen = StarlightWorld.vitricBiome.Intersects(new Rectangle((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16));

			return detectionBox.Contains((player.position / 16).ToPoint());
		}

		public override void OnInBiome(Player player)
		{
			if (player != Main.LocalPlayer)
				return;

			// Calculate and spawn new particles
			Vector2 basepoint = StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(-2000, 0);
			int screenCenterX = (int)(Main.screenPosition.X + Main.screenWidth / 2);

			int start = (int)(screenCenterX - basepoint.X) - (int)(Main.screenWidth * 1.5f);
			int end = (int)(screenCenterX - basepoint.X) + (int)(Main.screenWidth * 1.5f);

			for (int k = start; k <= end; k += 30)
			{
				if (Main.bloodMoon)
				{
					if (Main.rand.NextBool(400))
						BackgroundParticles.AddParticle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(0.3f, 1.3f)), 0, 0, Color.Red, 2400, basepoint + new Vector2(2000 + Main.rand.Next(12000), 0), default, 1, Main.rand.Next(100));

					if (Main.rand.NextBool(1300))
						ForegroundParticles.AddParticle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(0.3f, 1.9f)), 0, 0, Color.Red, 1800, basepoint + new Vector2(2000 + Main.rand.Next(12000), 0), default, 1, Main.rand.Next(50));
				}
				else
				{
					if (Main.rand.NextBool(400))
						BackgroundParticles.AddParticle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(-1.3f, -0.3f)), 0, 0, Color.White, 2400, basepoint + new Vector2(2000 + Main.rand.Next(12000), 1800), default, 1, Main.rand.Next(100));

					if (Main.rand.NextBool(1300))
						ForegroundParticles.AddParticle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(-1.9f, -0.3f)), 0, 0, Color.White, 1800, basepoint + new Vector2(2000 + Main.rand.Next(12000), 1800), default, 1, Main.rand.Next(50));
				}
			}

			// Update particles
			ForegroundParticles.UpdateParticles();
			BackgroundParticles.UpdateParticles();

			if (Main.Configuration.Get<bool>("UseHeatDistortion", false) && !NPC.AnyDanger())
			{
				if (!Filters.Scene["GradientDistortion"].IsActive())
				{
					Filters.Scene["GradientDistortion"].GetShader().Shader.Parameters["uZoom"].SetValue(Main.GameViewMatrix.Zoom);
					Filters.Scene.Activate("GradientDistortion").GetShader()
						.UseOpacity(2.5f)
						.UseIntensity(7f)
						.UseProgress(6)
						.UseImage(LightingBuffer.screenLightingTarget.RenderTarget, 0);
				}
			}
			else
			{
				if (Filters.Scene["GradientDistortion"].IsActive())
					Filters.Scene.Deactivate("GradientDistortion");
			}
		}

		public override void OnLeave(Player player)
		{
			if (Filters.Scene["GradientDistortion"].IsActive())
				Filters.Scene.Deactivate("GradientDistortion");
		}

		/// <summary>
		/// This method forces DrawBlack to be called while in the biome to ensure correct rendering
		/// of the area with the passive light, similar to hell
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="force"></param>
		private void ForceDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
		{
			if (onScreen)
				orig(self, true);
			else
				orig(self, force);
		}

		/// <summary>
		/// This IL edit changes the threshold for DrawBlack to render, this is needed to ensure
		/// that black squares dont appear in thin air.
		/// </summary>
		/// <param name="il"></param>
		private void ChangeBlackThreshold(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdloc(6), n => n.MatchStloc(13)); //beginning of the loop, local 11 is a looping variable
			c.Index++; //this is kinda goofy since I dont think you could actually ever write c# to compile to the resulting IL from emitting here.
			c.Emit(OpCodes.Ldloc, 3); //pass the original value so we can set that instead if we dont want to change the threshold
			c.EmitDelegate<Func<float, float>>(NewThreshold); //check if were in the biome to set, else set the original value
			c.Emit(OpCodes.Stloc, 3); //num2 in vanilla, controls minimum threshold to turn a tile black
		}

		/// <summary>
		/// This is called by the ChangeBlackThreshold IL edit to get the appropriate threshold
		/// </summary>
		/// <param name="orig">The original threshold value, to return if not in the biome</param>
		/// <returns>The threshold to use</returns>
		private float NewThreshold(float orig)
		{
			if (onScreen)
				return 0.01f;
			else
				return orig;
		}

		/// <summary>
		/// Particle update delegate for the foreground system
		/// </summary>
		/// <param name="particle"></param>
		private static void UpdateForegroundBody(Particle particle)
		{
			particle.Timer--;
			particle.StoredPosition += particle.Velocity;

			float randTime = particle.Type + 200f;
			float prog = particle.Timer / 1800f;
			float alpha = 0.85f * prog;

			particle.Position.X = particle.StoredPosition.X + GetParallaxOffset(particle.StoredPosition.X, 0.15f) + MathF.Sin(particle.Timer / randTime * 6.28f) * 20;
			particle.Position.Y = particle.StoredPosition.Y + GetParallaxOffsetY(particle.StoredPosition.Y, 0.1f);

			//particle.Color = Color.Lerp(new Color(255, 40, 0), new Color(255, 170, 100), prog) * (0.85f * prog);
			particle.Color.R = (byte)(255 * alpha);
			particle.Color.G = (byte)((40 + 130 * prog) * alpha);
			particle.Color.B = (byte)(100 * prog * alpha);

			particle.Scale = prog * 0.55f;
			particle.Rotation += 0.015f;
		}

		/// <summary>
		/// Particle update delegate for the background system
		/// </summary>
		/// <param name="particle"></param>
		private static void UpdateBackgroundBody(Particle particle)
		{
			particle.Timer--;
			particle.StoredPosition += particle.Velocity;

			float randTime = particle.Type + 100f;
			float prog = particle.Timer / 2400f;

			particle.Position.X = particle.StoredPosition.X + GetParallaxOffset(particle.StoredPosition.X, 0.5f) + MathF.Sin(particle.Timer / randTime * 6.28f) * 6;
			particle.Position.Y = particle.StoredPosition.Y + GetParallaxOffsetY(particle.StoredPosition.Y, 0.2f);

			//particle.Color = Color.Lerp(Color.Red, new Color(255, 255, 200), prog);
			particle.Color.R = 255;
			particle.Color.G = (byte)(255 * prog);
			particle.Color.B = (byte)(200 * prog);

			particle.Scale = prog;
			particle.Rotation += 0.02f;
		}

		/// <summary>
		/// This detour acts as the main function responsible for drawing the background in the desert
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		private void DrawVitricBackground(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
		{
			orig(self);

			// If we're in an invalid state to draw, such as on the menu, are a dedserv, or are not in the biome, dont!
			if (Main.gameMenu || Main.dedServ || !onScreen)
				return;

			parallaxOrigin = Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;
			vanillaParallax = 1 - (Main.caveParallax - 0.8f) / 0.2f;

			Vector2 basepoint = (StarlightWorld.vitricBiome != default) ? StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(-2000, 0) : Vector2.Zero;

			float x = basepoint.X + GetParallaxOffset(basepoint.X, 0.6f) - Main.screenPosition.X;
			float y = basepoint.Y + GetParallaxOffsetY(basepoint.Y, 0.2f) - Main.screenPosition.Y;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			DrawLayer(basepoint, textures[5].Value, 6, Vector2.UnitY * 100, default, false);
			DrawLayer(basepoint, textures[4].Value, 5, Vector2.UnitY * 40, default, false);
			BackgroundParticles.DrawParticles(Main.spriteBatch);
			DrawLayer(basepoint, textures[3].Value, 4, Vector2.UnitY * 150, default, false);
			DrawLayer(basepoint, textures[2].Value, 3, Vector2.UnitY * 160, default, false);
			DrawLayer(basepoint, textures[1].Value, 2, Vector2.UnitY * 355, default, false);
			DrawLayer(basepoint, Assets.Backgrounds.GlassTowerLoop.Value, 2, new Vector2(1304, 107), default, false);
			ForegroundParticles.DrawParticles(Main.spriteBatch);
			DrawLayer(basepoint, textures[0].Value, 1, Vector2.UnitY * 380, default, false);

			float progress = (float)Math.Sin(Main.GameUpdateCount / 50f);
			var color = new Color(255, 255, 100, 0);
			float colorAdd = 0f;

			if (!Main.dayTime)
				colorAdd = Math.Min(2, (float)Math.Sin(Main.time / Main.nightLength) * 5.0f);

			DrawLayer(basepoint, Assets.Backgrounds.Glass0Glow.Value, 1, Vector2.UnitY * 380 + Vector2.One * progress * 2, color * (0.45f + (progress + colorAdd) * 0.2f), false);
			DrawLayer(basepoint, Assets.Backgrounds.Glass0Glow.Value, 1, Vector2.UnitY * 380 + Vector2.One.RotatedBy(MathHelper.PiOver2) * progress * 2, color * (0.45f + (progress + colorAdd) * 0.2f), false);

			// Restart and draw the tiling background
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			DrawTilingBackground();
		}

		/// <summary>
		/// This is used for the title screen
		/// </summary>
		public static void DrawTitleVitricBackground()
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

			for (int k = 5; k >= 0; k--)
			{
				if (k == 3)
				{
					BackgroundParticles.DrawParticles(Main.spriteBatch);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);
				}

				Texture2D tex = textures[k].Value;

				float heightRatio = Main.screenHeight / (float)Main.screenWidth;
				int width = (int)(tex.Width * heightRatio);
				var pos = new Vector2((int)(Main.screenPosition.X * 0.035f * -(k - 5)) % width, 0);

				Color color = Color.White;

				byte a = color.A;

				color *= 0.8f + (Main.dayTime ? (float)Math.Sin(Main.time / Main.dayLength * 3.14f) * 0.35f : -(float)Math.Sin(Main.time / Main.nightLength * 3.14f) * 0.35f);
				color.A = a;

				for (int h = 0; h < Main.screenWidth + width; h += width)//during loading the texture has a width of one
					Main.spriteBatch.Draw(tex, new Rectangle(h - (int)pos.X, (int)pos.Y, width, Main.screenHeight), null, color, 0, default, 0, 0);

				if (k == 0)
				{
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

					float progress = (float)Math.Sin(Main.screenPosition.X * 0.005f);

					float colorAdd = 0f;

					if (!Main.dayTime)
						colorAdd = Math.Min(2, (float)Math.Sin(Main.time / Main.nightLength * 3.14f) * 5.0f);//causes the brightness to jump on the title screen

					Color color2 = new Color(255, 255, 100) * (0.45f + (progress + colorAdd) * 0.2f);

					for (float h = 0; h < Main.screenWidth + width; h += width)
					{
						Texture2D texGlow = Assets.Backgrounds.Glass0Glow.Value;
						var rect = new Rectangle((int)(h - pos.X), (int)pos.Y, width, Main.screenHeight);
						Main.spriteBatch.Draw(texGlow, rect, null, color2, 0, Vector2.UnitY + Vector2.One * progress * 2, 0, 0);
						Main.spriteBatch.Draw(texGlow, rect, null, color2, 0, Vector2.One.RotatedBy(MathHelper.PiOver2) * progress * 2, 0, 0);
					}

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);
				}

				if (k == 1)
				{
					ForegroundParticles.DrawParticles(Main.spriteBatch);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);
				}
			}

			int screenCenterX = (int)(Main.screenPosition.X + Main.screenWidth / 2);
			for (int k = (int)(screenCenterX - Main.screenPosition.X) - (int)(Main.screenWidth * 1.5f); k <= (int)(screenCenterX - Main.screenPosition.X) + (int)(Main.screenWidth * 1.5f); k += 30)
			{
				Vector2 spawnPos = Main.screenPosition + new Vector2(Main.rand.Next(Main.screenWidth * 2), Main.screenHeight + 0);
				if (Main.rand.NextBool(800))
					BackgroundParticles.AddParticle(new Vector2(0, Main.screenPosition.Y + 1600), new Vector2(3f, Main.rand.NextFloat(-1.3f, -0.3f)), 0, 0, Color.White, 2400, spawnPos);

				if (Main.rand.NextBool(2600))
					ForegroundParticles.AddParticle(new Vector2(0, Main.screenPosition.Y + 1600), new Vector2(3f, Main.rand.NextFloat(-1.9f, -0.3f)), 0, 0, Color.White, 1800, spawnPos);
			}
		}

		/// <summary>
		/// This draws the tiling sandy background using the lighting buffer
		/// </summary>
		private void DrawTilingBackground()
		{
			Texture2D tex = Assets.Backgrounds.VitricSand.Value;
			Texture2D texBot = Assets.Backgrounds.VitricSandBottom.Value;
			Texture2D texTop = Assets.Backgrounds.VitricSandTop.Value;
			Texture2D texLeft = Assets.Backgrounds.VitricSandLeft.Value;
			Texture2D texRight = Assets.Backgrounds.VitricSandRight.Value;

			var blacklist = new Rectangle(StarlightWorld.vitricBiome.X, StarlightWorld.vitricBiome.Y - 2, StarlightWorld.vitricBiome.Width, StarlightWorld.vitricBiome.Height);

			for (int x = -tex.Width; x <= Main.screenWidth + tex.Width; x += tex.Width)
			{
				for (int y = -tex.Height; y <= Main.screenHeight + tex.Height; y += tex.Height)
				{
					Vector2 pos = new Vector2(x, y) - new Vector2(Main.screenPosition.X % tex.Width, Main.screenPosition.Y % tex.Height);
					Texture2D drawtex;
					if (!CheckBackground(pos + new Vector2(0, tex.Height), tex.Size(), blacklist, true))
						drawtex = texTop;
					else if (!CheckBackground(pos + new Vector2(0, -tex.Height), tex.Size(), blacklist, true))
						drawtex = texBot;
					else if (!CheckBackground(pos + new Vector2(-tex.Width, 0), tex.Size(), blacklist, true))
						drawtex = texRight;
					else if (!CheckBackground(pos + new Vector2(tex.Width, 0), tex.Size(), blacklist, true))
						drawtex = texLeft;
					else
						drawtex = tex;

					if (CheckBackground(pos, drawtex.Size(), blacklist))
						LightingBufferRenderer.DrawWithLighting(drawtex, pos, Color.White);
				}
			}
		}

		/// <summary>
		/// Helper method to check if a tiling background tile can be drawn
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="size"></param>
		/// <param name="biome"></param>
		/// <param name="dontCheckScreen"></param>
		/// <returns></returns>
		private bool CheckBackground(Vector2 pos, Vector2 size, Rectangle biome, bool dontCheckScreen = false)
		{
			if (dontCheckScreen || ScreenTracker.OnScreenScreenspace(new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y)))
			{
				if (!Main.BackgroundEnabled)
					return true;
				else if (!biome.Contains(((pos + Main.screenPosition) / 16).ToPoint()) || !biome.Contains(((pos + size + Main.screenPosition) / 16).ToPoint()))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Draws a single layer of the vitric desert background
		/// </summary>
		/// <param name="basepoint">The base pos to calculate parallax from</param>
		/// <param name="texture">The texture for this layer</param>
		/// <param name="parallax">The parallax offset to calculate parallax</param>
		/// <param name="off">Absolute offset to add to this layer</param>
		/// <param name="color">The color to draw this layer in</param>
		/// <param name="flip">If this layer is flipped or not</param>
		private static void DrawLayer(Vector2 basepoint, Texture2D texture, float parallax, Vector2 off = default, Color color = default, bool flip = false)
		{
			if (color == default)
			{
				color = Color.White;

				byte a = color.A;

				color *= 0.8f + (Main.dayTime ? (float)Math.Sin(Main.time / Main.dayLength * 3.14f) * 0.35f : -(float)Math.Sin(Main.time / Main.nightLength * 3.14f) * 0.35f);
				color.A = a;
			}

			for (int k = 0; k <= 5; k++)
			{
				float x = basepoint.X + off.X + k * 739 * 4 + GetParallaxOffset(basepoint.X, parallax * 0.1f) - (int)Main.screenPosition.X;
				float y = basepoint.Y + off.Y - (int)Main.screenPosition.Y + GetParallaxOffsetY(basepoint.Y + StarlightWorld.vitricBiome.Height * 8, parallax * 0.04f);

				if (x > -texture.Width && x < Main.screenWidth + 30)
					Main.spriteBatch.Draw(texture, new Vector2(x, y), null, color, 0f, Vector2.Zero, 1f, flip ? SpriteEffects.FlipVertically : 0, 0);
			}
		}

		private static int GetParallaxOffset(float startpoint, float factor)
		{
			return (int)((parallaxOrigin.X - startpoint) * factor * vanillaParallax);
		}

		private static int GetParallaxOffsetY(float startpoint, float factor)
		{
			return (int)((parallaxOrigin.Y - startpoint) * factor);
		}
	}
}