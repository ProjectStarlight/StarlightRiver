﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Helpers;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
	class VitricBackground : HookGroup
	{
		internal static ParticleSystem ForegroundParticles;
		internal static ParticleSystem BackgroundParticles;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			ForegroundParticles = new ParticleSystem("StarlightRiver/Assets/GUI/HolyBig", UpdateForegroundBody, ParticleSystem.AnchorOptions.World);
			BackgroundParticles = new ParticleSystem("StarlightRiver/Assets/GUI/Holy", UpdateBackgroundBody, ParticleSystem.AnchorOptions.World);

			On_Main.DrawBackgroundBlackFill += DrawVitricBackground;
			On_Main.DrawBlack += ForceDrawBlack;
			IL_Main.DrawBlack += ChangeBlackThreshold;
		}

		public override void Unload()
		{
			ForegroundParticles ??= null;
			BackgroundParticles ??= null;
		}

		private void ForceDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
		{
			if (StarlightWorld.vitricBiome.Intersects(Helper.ScreenTiles))
				orig(self, true);
			else
				orig(self, force);
		}

		private void ChangeBlackThreshold(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdloc(6), n => n.MatchStloc(13)); //beginning of the loop, local 11 is a looping variable
			c.Index++; //this is kinda goofy since I dont think you could actually ever write c# to compile to the resulting IL from emitting here.
			c.Emit(OpCodes.Ldloc, 3); //pass the original value so we can set that instead if we dont want to change the threshold
			c.EmitDelegate<Func<float, float>>(NewThreshold); //check if were in the biome to set, else set the original value
			c.Emit(OpCodes.Stloc, 3); //num2 in vanilla, controls minimum threshold to turn a tile black
		}

		private float NewThreshold(float orig)
		{
			if (StarlightWorld.vitricBiome.Intersects(Helper.ScreenTiles))
				return 0.01f;
			else
				return orig;
		}

		private static void UpdateForegroundBody(Particle particle)
		{
			particle.Timer--;
			particle.StoredPosition += particle.Velocity;

			float randTime = particle.GetHashCode() % 100 + 200f;

			particle.Position.X = particle.StoredPosition.X + GetParallaxOffset(particle.StoredPosition.X, 0.15f) + (float)Math.Sin(particle.Timer / randTime * 6.28f) * 20;
			particle.Position.Y = particle.StoredPosition.Y + GetParallaxOffsetY(particle.StoredPosition.Y, 0.1f);

			particle.Color = Color.Lerp(new Color(255, 40, 0), new Color(255, 170, 100), particle.Timer / 1800f) * (0.85f * particle.Timer / 1800f);
			particle.Scale = particle.Timer / 1800f * 0.55f;
			particle.Rotation += 0.015f;
		}

		private static void UpdateBackgroundBody(Particle particle)
		{
			particle.Timer--;
			particle.StoredPosition += particle.Velocity;
			float randTime = particle.GetHashCode() % 50 + 100f;
			particle.Position.X = particle.StoredPosition.X + GetParallaxOffset(particle.StoredPosition.X, 0.5f) + (float)Math.Sin(particle.Timer / randTime * 6.28f) * 6;
			particle.Position.Y = particle.StoredPosition.Y + GetParallaxOffsetY(particle.StoredPosition.Y, 0.2f);
			particle.Color = Color.Lerp(Color.Red, new Color(255, 255, 200), particle.Timer / 2400f);
			particle.Scale = particle.Timer / 2400f;
			particle.Rotation += 0.02f;
		}

		private void DrawVitricBackground(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
		{
			orig(self);

			if (Main.gameMenu || Main.dedServ)
				return;

			Player Player = Main.LocalPlayer;

			if (Player != null && StarlightWorld.vitricBiome.Intersects(Helper.ScreenTiles))
			{
				Vector2 basepoint = (StarlightWorld.vitricBiome != default) ? StarlightWorld.vitricBiome.TopLeft() * 16 + new Vector2(-2000, 0) : Vector2.Zero;

				float x = basepoint.X + GetParallaxOffset(basepoint.X, 0.6f) - Main.screenPosition.X;
				float y = basepoint.Y + GetParallaxOffsetY(basepoint.Y, 0.2f) - Main.screenPosition.Y;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

				for (int k = 5; k >= 0; k--)
				{
					if (k == 3)
						BackgroundParticles.DrawParticles(Main.spriteBatch);

					int off = 140 + (340 - k * 110);
					if (k == 0)
						off -= 100;
					if (k == 1)
						off -= 25;
					if (k == 2)
						off -= 100;
					if (k == 5)
						off = 100;

					DrawLayer(basepoint, Request<Texture2D>("StarlightRiver/Assets/Backgrounds/Glass" + k).Value, k + 1, Vector2.UnitY * off, default, false); //the crystal layers and front sand

					if (k == 1)
					{
						DrawLayer(basepoint, Assets.Backgrounds.GlassTowerLoop.Value, k + 1, new Vector2(1304, off - 248), default, false);
					}

					if (k == 0)
					{
						Main.spriteBatch.End();
						Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

						float progress = (float)Math.Sin(Main.GameUpdateCount / 50f);
						var color = new Color(255, 255, 100);
						float colorAdd = 0f;

						if (!Main.dayTime)
							colorAdd = Math.Min(2, (float)Math.Sin(Main.time / Main.nightLength) * 5.0f);

						DrawLayer(basepoint, Assets.Backgrounds.Glass0Glow.Value, k + 1, Vector2.UnitY * off + Vector2.One * progress * 2, color * (0.45f + (progress + colorAdd) * 0.2f), false);
						DrawLayer(basepoint, Assets.Backgrounds.Glass0Glow.Value, k + 1, Vector2.UnitY * off + Vector2.One.RotatedBy(MathHelper.PiOver2) * progress * 2, color * (0.45f + (progress + colorAdd) * 0.2f), false);

						Main.spriteBatch.End();
						Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
					}

					if (k == 1)
						ForegroundParticles.DrawParticles(Main.spriteBatch);
				}

				int screenCenterX = (int)(Main.screenPosition.X + Main.screenWidth / 2);
				for (int k = (int)(screenCenterX - basepoint.X) - (int)(Main.screenWidth * 1.5f); k <= (int)(screenCenterX - basepoint.X) + (int)(Main.screenWidth * 1.5f); k += 30)
				{
					if (Main.bloodMoon)
					{
						Vector2 spawnPos = basepoint + new Vector2(2000 + Main.rand.Next(12000), 0);
						if (Main.rand.NextBool(400))
							BackgroundParticles.AddParticle(new Particle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(0.3f, 1.3f)), 0, 0, Color.Red, 2400, spawnPos));

						if (Main.rand.NextBool(1300))
							ForegroundParticles.AddParticle(new Particle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(0.3f, 1.9f)), 0, 0, Color.Red, 1800, spawnPos));
					}
					else
					{
						Vector2 spawnPos = basepoint + new Vector2(2000 + Main.rand.Next(12000), 1800);
						if (Main.rand.NextBool(400))
							BackgroundParticles.AddParticle(new Particle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(-1.3f, -0.3f)), 0, 0, Color.White, 2400, spawnPos));

						if (Main.rand.NextBool(1300))
							ForegroundParticles.AddParticle(new Particle(new Vector2(0, basepoint.Y + 1600), new Vector2(0, Main.rand.NextFloat(-1.9f, -0.3f)), 0, 0, Color.White, 1800, spawnPos));
					}
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
				DrawTilingBackground();
			}
		}

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

				Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Backgrounds/Glass" + k).Value;

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
					BackgroundParticles.AddParticle(new Particle(new Vector2(0, Main.screenPosition.Y + 1600), new Vector2(3f, Main.rand.NextFloat(-1.3f, -0.3f)), 0, 0, Color.White, 2400, spawnPos));

				if (Main.rand.NextBool(2600))
					ForegroundParticles.AddParticle(new Particle(new Vector2(0, Main.screenPosition.Y + 1600), new Vector2(3f, Main.rand.NextFloat(-1.9f, -0.3f)), 0, 0, Color.White, 1800, spawnPos));
			}
		}

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
						LightingBufferRenderer.DrawWithLighting(pos, drawtex);
				}
			}
		}

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
			float vanillaParallax = 1 - (Main.caveParallax - 0.8f) / 0.2f;
			return (int)((Main.screenPosition.X + Main.screenWidth / 2 - startpoint) * factor * vanillaParallax);
		}

		private static int GetParallaxOffsetY(float startpoint, float factor)
		{
			return (int)((Main.screenPosition.Y + Main.screenHeight / 2 - startpoint) * factor);
		}
	}
}