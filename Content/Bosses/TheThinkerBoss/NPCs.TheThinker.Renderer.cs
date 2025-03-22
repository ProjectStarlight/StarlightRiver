using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using static tModPorter.ProgressUpdate;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal partial class TheThinker
	{
		private List<Vector2> arenaCache;
		private Trail arenaTrail;

		public int shellFrame = 0;

		public int pulseTime;
		public float arenaFade;

		public float pulseProg => pulseTime > 25 ? 1f - (pulseTime - 25) / 5f : pulseTime / 25f;
		public float ArenaOpacity => arenaFade / 120f;

		public float FakeBrainRadius
		{
			get
			{
				float rad = 0.8f;

				if (Main.GameUpdateCount % 300 > 200)
				{
					rad = 0.8f - 0.4f * Helpers.Eases.BezierEase((Main.GameUpdateCount % 300 - 200) / 80f);
				}

				if (Main.GameUpdateCount % 300 > 280)
				{
					rad = 0.4f + 0.4f * Helpers.Eases.SwoopEase((Main.GameUpdateCount % 300 - 280) / 20f);
				}

				return rad;
			}
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				DrawBestiary(spriteBatch, screenPos);
				return false;
			}

			if (!Open)
				return false;

			DrawCocoon();

			if (active)
				DrawArenaBorder(spriteBatch);

			return false;
		}

		/// <summary>
		/// Renders the backgrounds for each appropriate thinker
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		private void DrawBackgrounds(On_Main.orig_DrawPlayers_BehindNPCs orig, Main self)
		{
			orig(self);

			foreach (TheThinker thinker in toRender)
			{
				thinker.DrawBackground(Main.spriteBatch);
			}
		}

		/// <summary>
		/// Draws the background element of the arena, using its fade-in shader
		/// </summary>
		/// <param name="spriteBatch"></param>
		public void DrawBackground(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Bosses.TheThinkerBoss.ThinkerBackground.Value;
			Effect backgroundShader = ShaderLoader.GetShader("ThinkerBackground").Value;

			if (backgroundShader != null)
			{
				backgroundShader.Parameters["u_screenSize"].SetValue(Main.ScreenSize.ToVector2());
				backgroundShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.ThinkerBackground.Size());
				backgroundShader.Parameters["u_time"].SetValue(ArenaOpacity);
				backgroundShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.7f, 0.7f));
				backgroundShader.Parameters["u_sampleTopLeft"].SetValue(home - tex.Size() / 2f - Main.screenPosition);

				backgroundShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.ThinkerBackground.Value);
				backgroundShader.Parameters["noise_t"].SetValue(Assets.Noise.ShaderNoiseLooping.Value);
				backgroundShader.Parameters["light_t"].SetValue(LightingBuffer.screenLightingTarget.RenderTarget);

				LightingBuffer.bufferNeedsPopulated = true;

				spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				var fade = Assets.Masks.GlowLarge.Value;
				spriteBatch.Draw(fade, home - Main.screenPosition, null, Color.Black * 0.3f * ArenaOpacity, 0, fade.Size() / 2f, 2000f / fade.Width, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, backgroundShader, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.Draw(tex, home - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);

				spriteBatch.End();
			}
		}

		/// <summary>
		/// Draws the thinkers 'cocoon'/shell. While its unintutive, even the part that appears above the heart actually draws under it,
		/// and then the actual hearts rendering is masked. This is so that the grayscale of the gray matter shader can apply to the
		/// shell but not the heart.
		/// </summary>
		public void DrawCocoon()
		{
			Texture2D tex = Assets.Bosses.TheThinkerBoss.ShellBack.Value;
			Texture2D texOver = Assets.Bosses.TheThinkerBoss.ShellFront.Value;

			Vector2 pos = home - Main.screenPosition - tex.Size() / 2f;

			Rectangle frame = new Rectangle(0, texOver.Height / 3 * shellFrame, texOver.Width, texOver.Height / 3);

			LightingBufferRenderer.DrawWithLighting(tex, pos, Color.Gray);
			LightingBufferRenderer.DrawWithLighting(texOver, pos, frame, Color.White);

			if (!FightActive)
			{
				Vector2 target = NPC.Center + new Vector2(0, 200);

				DeadBrain.DrawBrainSegments(Main.spriteBatch, NPC, target - Main.screenPosition, Lighting.GetColor((target / 16).ToPoint()), 0, 1, 1, radiusOverride: FakeBrainRadius);

				Lighting.AddLight(target, new Vector3(0.5f, 0.35f, 0.35f));
			}
		}

		/// <summary>
		/// Draws the arena border
		/// </summary>
		/// <param name="spriteBatch"></param>
		public void DrawArenaBorder(SpriteBatch spriteBatch)
		{
			Texture2D spike = Assets.Misc.SpikeTell.Value;
			Texture2D solid = Assets.Bosses.TheThinkerBoss.ShellSpike.Value;

			for (int k = 0; k < 36; k++)
			{
				float offset = (float)Math.Sin(k / 36f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 15;
				offset += (float)Math.Cos(k / 36f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 15;

				float rot = k / 36f * 6.28f;
				Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (ArenaRadius + 50 + offset);
				spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 60, 0) * 0.25f * ArenaOpacity, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
				spriteBatch.Draw(solid, edge - Main.screenPosition, new Rectangle(0, 0, 58, 60), new Color(Lighting.GetSubLight(edge)) * ArenaOpacity, rot - 1.57f / 2f, new Vector2(58, 60), 1f, 0, 0);

				offset = (float)Math.Sin((k + 0.5) / 36f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 15;
				offset += (float)Math.Cos((k + 0.5) / 36f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 15;
				rot = (k + 0.5f) / 36f * 6.28f;
				float sin = (float)Math.Sin(Main.GameUpdateCount * 0.01f + k);
				edge = home + Vector2.UnitX.RotatedBy(rot) * (ArenaRadius + 60 + sin * 8 + offset);
				spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 60, 0) * 0.25f * (1f - sin + 0.5f) * ArenaOpacity, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
				spriteBatch.Draw(solid, edge - Main.screenPosition, new Rectangle(58, 0, 58, 60), new Color(Lighting.GetSubLight(edge)) * ArenaOpacity, rot - 1.57f / 2f, new Vector2(58, 60), 1f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			ManageArenaTrail();
			DrawArenaEdgeTrail();
		}

		/// <summary>
		/// Updates the trail used to track the pulsing shape of the arena border
		/// </summary>
		protected void ManageArenaTrail()
		{
			if (arenaCache is null || arenaCache.Count != 73)
				arenaCache = new(new Vector2[73]);

			for (int k = 0; k < 72; k++)
			{
				float rot = k / 72f * 6.28f;
				float offset = (float)Math.Sin(k / 72f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 12;
				offset += (float)Math.Cos(k / 72f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 12;

				arenaCache[k] = home + Vector2.UnitX.RotatedBy(rot) * (ArenaRadius + 50 + offset);
			}

			arenaCache[72] = arenaCache[0];

			if (arenaTrail is null || arenaTrail.IsDisposed)
			{
				arenaTrail = new Trail(Main.instance.GraphicsDevice, 73, new NoTip(), factor => 56, factor =>
				{
					int index = (int)(factor.X * 72);
					return new Color(Lighting.GetSubLight(arenaCache[index])) * ArenaOpacity;
				});
			}

			arenaTrail.Positions = arenaCache.ToArray();
			arenaTrail.NextPosition = NPC.Center;
		}

		/// <summary>
		/// Draws the trail portion of the arena edge
		/// </summary>
		public void DrawArenaEdgeTrail()
		{
			Effect effect = ShaderLoader.GetShader("RepeatingChain").Value;

			var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["alpha"].SetValue(1f);
			effect.Parameters["repeats"].SetValue(81f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(Assets.Bosses.TheThinkerBoss.ArenaSegment.Value);
			arenaTrail?.Render(effect);
		}

		/// <summary>
		/// Draws the graymatter mask for each thinker
		/// </summary>
		/// <param name="sb"></param>
		private void DrawGrayAura(SpriteBatch sb)
		{
			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			Color color = Color.White;
			color.A = 0;

			foreach (TheThinker thinker in toRender)
			{
				float rad = 140 + thinker.ExtraGrayAuraRadius;

				if (rad >= 1)
				{
					for (int k = 0; k < 8; k++)
					{
						sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, rad * 4 / glow.Width, 0, 0);
					}
				}
			}

			toRender.RemoveAll(n => n is null || !n.NPC.active);
		}

		/// <summary>
		/// Draws ALL thinkers that are ready to render
		/// </summary>
		/// <param name="sb"></param>
		private void DrawShadedBody(SpriteBatch sb)
		{
			if (!Open)
				return;

			foreach (TheThinker thinker in toRender)
			{
				if (thinker.ThisBrain?.Phase == DeadBrain.Phases.ReallyDead)
				{
					thinker.DrawFlowerToDead(sb, thinker.ThisBrain.Timer);
					continue;
				}

				if (thinker.bloomProgress == 0)
					thinker.DrawHeart(sb, 1, Main.screenPosition);
				else
					thinker.DrawHeartToFlower(sb, thinker.bloomProgress, Main.screenPosition);
			}
		}

		public float Heartbeat(float t)
		{
			float omega = 2 * MathF.PI;
			float alpha = 0.5f;
			float beta = 2 * MathF.PI;

			float pulse = MathF.Sin(omega * t);
			float decay = MathF.Exp(-alpha * (t % (2 * MathF.PI / omega)));
			float modulation = 1 + MathF.Cos(beta * t);

			return MathF.Pow(pulse, 2) * decay * modulation;
		}

		/// <summary>
		/// Renders the normal "heart" appearance of the thinker
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="scale"></param>
		private void DrawHeart(SpriteBatch spriteBatch, float scale, Vector2 screenPos)
		{
			Texture2D glow = Assets.Masks.Glow.Value;

			spriteBatch.Draw(glow, NPC.Center - screenPos, null, Color.Black * 0.5f, NPC.rotation, glow.Size() / 2f, NPC.scale * 2.8f, 0, 0);

			// need a scissor enabled rasterizer to be able to draw in bestiary
			var rasterizer = new RasterizerState() { ScissorTestEnable = true };

			var scaleCalc = 1f + 0.2f * Heartbeat(Main.GameUpdateCount * 0.02f);

			Effect bodyShader = ShaderLoader.GetShader("ThinkerBody").Value;

			if (bodyShader != null)
			{
				bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.Heart.Size() * scaleCalc);
				bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.Heart.Value);
				bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartLine.Value);
				bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
				bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);
				bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartNormal.Value);
				bodyShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f) * scale);
				bodyShader.Parameters["u_fade"].SetValue(Vector3.Lerp(new Vector3(0.0f, 0.2f, 0.4f), new Vector3(0.3f, 0.5f, 0.3f), scale)); // Lerp here so this is the same as the flower core at 0 scale
				bodyShader.Parameters["mask_t"].SetValue(shellFrame != 1 ? Assets.MagicPixel.Value : Assets.Bosses.TheThinkerBoss.CrackMask.Value);

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, rasterizer, bodyShader, Main.GameViewMatrix.TransformationMatrix);

				Texture2D tex = Assets.Bosses.TheThinkerBoss.Heart.Value;
				spriteBatch.Draw(tex, NPC.Center - screenPos, null, Color.White, NPC.rotation, tex.Size() / 2f, scaleCalc * NPC.scale, 0, 0);

				if (pulseTime > 0)
				{
					bodyShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f) * pulseProg);
					spriteBatch.Draw(tex, NPC.Center - screenPos, null, Color.White, NPC.rotation, tex.Size() / 2f, NPC.scale + pulseProg * 0.3f, 0, 0);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		/// <summary>
		/// Renders the final phase flower appearance for this thinker
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="scale">progress into the expansion animation, to be able to run forwards or backwards</param>
		private void DrawFlower(SpriteBatch spriteBatch, float scale, Vector2 screenPos)
		{
			Effect petalShader = ShaderLoader.GetShader("ThinkerPetal").Value;
			Effect bodyShader = ShaderLoader.GetShader("ThinkerBody").Value;

			if (petalShader != null && bodyShader != null)
			{
				petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.PetalSmall.Size());
				petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				petalShader.Parameters["mainbody_t"].SetValue(Assets.GlowTrail.Value);
				petalShader.Parameters["linemap_t"].SetValue(Assets.GlowTopTrail.Value);
				petalShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
				petalShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);

				Vector2 pos = NPC.Center - screenPos;

				Texture2D glow = Assets.Masks.Glow.Value;
				spriteBatch.Draw(glow, pos, null, Color.Black * 0.5f * scale, NPC.rotation, glow.Size() / 2f, NPC.scale * 5.5f, 0, 0);

				// need a scissor enabled rasterizer to be able to draw in bestiary
				var rasterizer = new RasterizerState() { ScissorTestEnable = true };

				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, rasterizer, petalShader, Main.GameViewMatrix.TransformationMatrix);

				Texture2D bigPetal = Assets.Bosses.TheThinkerBoss.PetalBig.Value;
				Texture2D smallPetal = Assets.Bosses.TheThinkerBoss.PetalSmall.Value;

				var bigOrigin = new Vector2(0, bigPetal.Height / 2f);
				var smallOrigin = new Vector2(0, smallPetal.Height / 2f);
				float baseRot = Main.GameUpdateCount * 0.01f;

				petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.Frond.Size());
				petalShader.Parameters["u_color"].SetValue(new Vector3(0.5f, 0.35f, 0.2f));
				petalShader.Parameters["u_fade"].SetValue(new Vector3(0.01f, 0.1f, 0.01f));

				for (int k = 0; k < 10; k++)
				{
					petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f + k * 0.1f);
					float thisScale = Eases.SwoopEase(Math.Clamp((scale - 0.3f) / 0.7f, 0, 1));
					float rot = baseRot + k / 10f * 6.28f + (k % 2 == 0 ? 0.1f : -0.1f) - 0.17f;
					spriteBatch.Draw(Assets.Bosses.TheThinkerBoss.Frond.Value, pos + Vector2.UnitX.RotatedBy(rot) * 42 * thisScale * NPC.scale, null, Color.White, rot, bigOrigin, thisScale * NPC.scale * new Vector2(1, 0.5f), 0, 0);
				}

				petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.PetalSmall.Size());
				petalShader.Parameters["u_color"].SetValue(new Vector3(0.4f, 0.6f, 0.4f));
				petalShader.Parameters["u_fade"].SetValue(new Vector3(0.1f, 0.2f, 0.4f));

				for (int k = 0; k < 5; k++)
				{
					petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * -0.01f + k * 0.2f);
					float thisScale = Eases.SwoopEase(Math.Clamp((scale - 0.15f) / 0.7f, 0, 1));
					float finalScale = thisScale + (float)Math.Sin(Main.GameUpdateCount * 0.1f + k * 0.25f) * 0.025f;
					float rot = baseRot + k / 5f * 6.28f;
					spriteBatch.Draw(smallPetal, pos + Vector2.UnitX.RotatedBy(rot) * 48 * thisScale * NPC.scale, null, Color.White, rot, smallOrigin, finalScale * NPC.scale, 0, 0);
				}

				petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.PetalBig.Size());
				petalShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f));
				petalShader.Parameters["u_fade"].SetValue(new Vector3(0.3f, 0.5f, 0.3f));

				for (int k = 0; k < 5; k++)
				{
					petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f + k * 0.2f);
					float thisScale = Eases.SwoopEase(Math.Clamp(scale / 0.7f, 0, 1));
					float finalScale = thisScale + (float)Math.Sin(Main.GameUpdateCount * 0.1f + k * -0.25f) * 0.05f;
					float rot = baseRot + k / 5f * 6.28f + 6.28f / 10f;
					spriteBatch.Draw(bigPetal, pos + Vector2.UnitX.RotatedBy(rot) * 32 * thisScale * NPC.scale, null, Color.White, rot, bigOrigin, finalScale * NPC.scale, 0, 0);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.Draw(glow, pos, null, Color.Black * (0.5f + 0.4f * scale), NPC.rotation, glow.Size() / 2f, NPC.scale * 2.8f, 0, 0);

				bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.FlowerCore.Size());
				bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.TheThinkerBoss.FlowerCore.Value);
				bodyShader.Parameters["linemap_t"].SetValue(Assets.GUI.RingGlow.Value);
				bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
				bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);
				bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.TheThinkerBoss.FlowerCoreNormal.Value);
				bodyShader.Parameters["u_color"].SetValue(new Vector3(0.9f, 0.7f, 0.2f) * Math.Min(scale / 0.2f, 1));
				bodyShader.Parameters["u_fade"].SetValue(new Vector3(0.0f, 0.2f, 0.4f) * Math.Min(scale / 0.2f, 1));
				bodyShader.Parameters["mask_t"].SetValue(Assets.MagicPixel.Value);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointWrap, default, rasterizer, bodyShader, Main.GameViewMatrix.TransformationMatrix);

				Texture2D coreTex = Assets.Bosses.TheThinkerBoss.FlowerCore.Value;
				spriteBatch.Draw(coreTex, pos, null, Color.White, NPC.rotation, coreTex.Size() / 2f, NPC.scale, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		/// <summary>
		/// Draws an additional ring of petals for the death animation
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="rotOffset"></param>
		/// <param name="scale"></param>
		/// <param name="color"></param>
		private void DrawExtraPetals(SpriteBatch spriteBatch, float rotOffset, float scale, Vector3 color, float rootOffset)
		{
			Effect petalShader = ShaderLoader.GetShader("ThinkerPetal").Value;

			if (petalShader != null)
			{
				petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);
				petalShader.Parameters["mainbody_t"].SetValue(Assets.GlowTrail.Value);
				petalShader.Parameters["linemap_t"].SetValue(Assets.GlowTopTrail.Value);
				petalShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
				petalShader.Parameters["overlay_t"].SetValue(Assets.Bosses.TheThinkerBoss.HeartOver.Value);
				petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.TheThinkerBoss.PetalBig.Size());
				petalShader.Parameters["u_color"].SetValue(color);
				petalShader.Parameters["u_fade"].SetValue(new Vector3(0.4f, 0.4f, 0.4f));

				Vector2 pos = NPC.Center - Main.screenPosition;
				Texture2D bigPetal = Assets.Bosses.TheThinkerBoss.PetalBig.Value;
				float baseRot = Main.GameUpdateCount * 0.01f + rotOffset;
				var bigOrigin = new Vector2(0, bigPetal.Height / 2f);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, petalShader, Main.GameViewMatrix.TransformationMatrix);

				for (int k = 0; k < 5; k++)
				{
					petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f + k * 0.2f);
					float thisScale = Eases.SwoopEase(scale);
					float finalScale = thisScale + (float)Math.Sin(Main.GameUpdateCount * 0.1f + k * -0.25f) * 0.05f;
					float rot = baseRot + k / 5f * 6.28f + 6.28f / 10f;
					spriteBatch.Draw(bigPetal, pos + Vector2.UnitX.RotatedBy(rot) * rootOffset * thisScale * NPC.scale, null, Color.White, rot, bigOrigin, finalScale * NPC.scale, 0, 0);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		/// <summary>
		/// Draws the death animation
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="progress"></param>
		private void DrawFlowerToDead(SpriteBatch spriteBatch, float progress)
		{
			float scaleMult = 1;

			if (progress > 240)
				scaleMult = 1 - Math.Max(0, Eases.BezierEase((progress - 240) / 60f));

			if (progress > 300)
				scaleMult = 0;

			NPC.scale = scaleMult;

			DrawFlower(spriteBatch, scaleMult, Main.screenPosition);

			if (progress > 90)
				DrawExtraPetals(spriteBatch, 0.4f, Math.Min(1, (progress - 90) / 60f) * 0.4f * scaleMult, new Vector3(0.6f, 0.4f, 0.2f), 32);

			if (progress > 120)
				DrawExtraPetals(spriteBatch, 0.8f, Math.Min(1, (progress - 120) / 60f) * 0.35f * scaleMult, new Vector3(0.4f, 0.6f, 0.4f), 24);

			if (progress > 150)
				DrawExtraPetals(spriteBatch, 1.2f, Math.Min(1, (progress - 150) / 50f) * 0.3f * scaleMult, new Vector3(0.2f, 0.4f, 0.6f), 16);

			if (progress > 180)
				DrawExtraPetals(spriteBatch, 1.6f, Math.Min(1, (progress - 180) / 40f) * 0.25f * scaleMult, new Vector3(0.0f, 0.2f, 0.6f), 8);

			if (progress > 210)
				DrawExtraPetals(spriteBatch, 2.0f, Math.Min(1, (progress - 210) / 30f) * 0.2f * scaleMult, new Vector3(0.0f, 0.1f, 0.8f), 0);

			return;
		}

		/// <summary>
		/// Draws the transition animation from heart to flower.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="progress">progress from 0 to 150, in frames. This can be reversed to do this animation backwards</param>
		private void DrawHeartToFlower(SpriteBatch spriteBatch, float progress, Vector2 screenPos)
		{
			if (progress < 60)
			{
				DrawHeart(spriteBatch, 1 - progress / 60f, screenPos);
			}
			else
			{
				DrawFlower(spriteBatch, (progress - 60f) / 90f, screenPos);
			}
		}

		/// <summary>
		/// Drawing specific to the bestiary
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="screenPos"></param>
		private void DrawBestiary(SpriteBatch sb, Vector2 screenPos)
		{
			uint time = Main.GameUpdateCount % 600;
			float timeToPass = 0f;

			if (time < 150)
				timeToPass = 0;
			else if (time < 300)
				timeToPass = time - 150;
			else if (time < 450)
				timeToPass = 150;
			else
				timeToPass = 150 - (time - 450);

			if (time > 190 && time <= 260)
				NPC.scale = 1f - Eases.BezierEase((time - 190) / 70f) * 0.4f;

			if (time > 500 && time < 550)
				NPC.scale = 0.6f + Eases.BezierEase((time - 500) / 50f) * 0.4f;

			DrawHeartToFlower(sb, timeToPass, screenPos);
		}
	}
}