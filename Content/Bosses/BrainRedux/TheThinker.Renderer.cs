using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using static tModPorter.ProgressUpdate;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class TheThinker
	{
		private List<Vector2> arenaCache;
		private Trail arenaTrail;

		public float arenaFade;
		public float ArenaOpacity => arenaFade / 120f;

		public int shellFrame = 0;

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}

		public void DrawUnderShell()
		{
			Texture2D tex = Assets.Bosses.BrainRedux.ShellBack.Value;
			Texture2D texOver = Assets.Bosses.BrainRedux.ShellFront.Value;

			Vector2 pos = home - Main.screenPosition - tex.Size() / 2f;

			Rectangle frame = new Rectangle(0, texOver.Height / 3 * shellFrame, texOver.Width, texOver.Height / 3);

			LightingBufferRenderer.DrawWithLighting(tex, pos, Color.Gray);
			LightingBufferRenderer.DrawWithLighting(texOver, pos, frame, Color.White);
		}

		public void DrawArena(SpriteBatch spriteBatch)
		{
			Texture2D spike = Assets.Misc.SpikeTell.Value;
			Texture2D solid = Assets.Bosses.BrainRedux.ShellSpike.Value;

			for (int k = 0; k < 36; k++)
			{
				float offset = (float)Math.Sin(k / 36f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 15;
				offset += (float)Math.Cos(k / 36f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 15;

				float rot = k / 36f * 6.28f;
				Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (ArenaRadius + 70 + offset);
				spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 60, 0) * 0.25f * ArenaOpacity, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
				spriteBatch.Draw(solid, edge - Main.screenPosition, new Rectangle(0, 0, 58, 60), new Color(Lighting.GetSubLight(edge)) * ArenaOpacity, rot - 1.57f / 2f, new Vector2(58, 60), 1f, 0, 0);

				offset = (float)Math.Sin((k + 0.5) / 36f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 15;
				offset += (float)Math.Cos((k + 0.5) / 36f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 15;
				rot = (k + 0.5f) / 36f * 6.28f;
				float sin = (float)Math.Sin(Main.GameUpdateCount * 0.01f + k);
				edge = home + Vector2.UnitX.RotatedBy(rot) * (ArenaRadius + 80 + sin * 10 + offset);
				spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 60, 0) * 0.25f * (1f - sin + 0.5f) * ArenaOpacity, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
				spriteBatch.Draw(solid, edge - Main.screenPosition, new Rectangle(58, 0, 58, 60), new Color(Lighting.GetSubLight(edge)) * ArenaOpacity, rot - 1.57f / 2f, new Vector2(58, 60), 1f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin();

			ManageArenaTrail();
			DrawArenaEdgeTrail();
		}

		protected void ManageArenaTrail()
		{
			if (arenaCache is null || arenaCache.Count != 73)
				arenaCache = new(new Vector2[73]);

			for (int k = 0; k < 72; k++)
			{
				float rot = k / 72f * 6.28f;
				float offset = (float)Math.Sin(k / 72f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 15;
				offset += (float)Math.Cos(k / 72f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 15;

				arenaCache[k] = home + Vector2.UnitX.RotatedBy(rot) * (ArenaRadius + 70 + offset);
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

		public void DrawArenaEdgeTrail()
		{
			Effect effect = Filters.Scene["RepeatingChain"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["alpha"].SetValue(1f);
			effect.Parameters["repeats"].SetValue(81f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			effect.Parameters["sampleTexture"].SetValue(Assets.Bosses.BrainRedux.ArenaSegment.Value);
			arenaTrail?.Render(effect);
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

			DrawUnderShell();

			if (active)
				DrawArena(spriteBatch);

			return false;
		}

		private void DrawGrayAura(SpriteBatch sb)
		{
			Texture2D glow = Assets.Keys.GlowAlpha.Value;
			Color color = Color.White;
			color.A = 0;

			foreach (TheThinker thinker in toRender)
			{
				for (int k = 0; k < 8; k++)
				{
					sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, (140 + thinker.ExtraGrayAuraRadius) * 4 / glow.Width, 0, 0);
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

		/// <summary>
		/// Renders the normal "heart" appearance of the thinker
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="scale"></param>
		private void DrawHeart(SpriteBatch spriteBatch, float scale, Vector2 screenPos)
		{
			Texture2D glow = Assets.Keys.Glow.Value;

			spriteBatch.Draw(glow, NPC.Center - screenPos, null, Color.Black * 0.5f, NPC.rotation, glow.Size() / 2f, NPC.scale * 2.8f, 0, 0);

			// need a scissor enabled rasterizer to be able to draw in bestiary
			var rasterizer = new RasterizerState() { ScissorTestEnable = true };

			bodyShader ??= Filters.Scene["ThinkerBody"].GetShader().Shader;

			bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.Heart.Size());
			bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

			bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.BrainRedux.Heart.Value);
			bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.BrainRedux.HeartLine.Value);
			bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
			bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.BrainRedux.HeartOver.Value);
			bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.BrainRedux.HeartNormal.Value);
			bodyShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f) * scale);
			bodyShader.Parameters["u_fade"].SetValue(Vector3.Lerp(new Vector3(0.0f, 0.2f, 0.4f), new Vector3(0.3f, 0.5f, 0.3f), scale)); // Lerp here so this is the same as the flower core at 0 scale
			bodyShader.Parameters["mask_t"].SetValue(shellFrame != 1 ? Assets.MagicPixel.Value : Assets.Bosses.BrainRedux.CrackMask.Value);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, rasterizer, bodyShader, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = Assets.Bosses.BrainRedux.Heart.Value;
			spriteBatch.Draw(tex, NPC.Center - screenPos, null, Color.White, NPC.rotation, tex.Size() / 2f, NPC.scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>
		/// Renders the final phase flower appearance for this thinker
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="scale">progress into the expansion animation, to be able to run forwards or backwards</param>
		private void DrawFlower(SpriteBatch spriteBatch, float scale, Vector2 screenPos)
		{
			petalShader ??= Filters.Scene["ThinkerPetal"].GetShader().Shader;
			bodyShader ??= Filters.Scene["ThinkerBody"].GetShader().Shader;

			petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.PetalSmall.Size());
			petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

			petalShader.Parameters["mainbody_t"].SetValue(Assets.GlowTrail.Value);
			petalShader.Parameters["linemap_t"].SetValue(Assets.GlowTopTrail.Value);
			petalShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
			petalShader.Parameters["overlay_t"].SetValue(Assets.Bosses.BrainRedux.HeartOver.Value);

			Vector2 pos = NPC.Center - screenPos;

			Texture2D glow = Assets.Keys.Glow.Value;
			spriteBatch.Draw(glow, pos, null, Color.Black * 0.5f * scale, NPC.rotation, glow.Size() / 2f, NPC.scale * 5.5f, 0, 0);

			// need a scissor enabled rasterizer to be able to draw in bestiary
			var rasterizer = new RasterizerState() { ScissorTestEnable = true };

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointWrap, default, rasterizer, petalShader, Main.GameViewMatrix.TransformationMatrix);

			Texture2D bigPetal = Assets.Bosses.BrainRedux.PetalBig.Value;
			Texture2D smallPetal = Assets.Bosses.BrainRedux.PetalSmall.Value;

			var bigOrigin = new Vector2(0, bigPetal.Height / 2f);
			var smallOrigin = new Vector2(0, smallPetal.Height / 2f);
			float baseRot = Main.GameUpdateCount * 0.01f;

			petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.Frond.Size());
			petalShader.Parameters["u_color"].SetValue(new Vector3(0.5f, 0.35f, 0.2f));
			petalShader.Parameters["u_fade"].SetValue(new Vector3(0.01f, 0.1f, 0.01f));

			for (int k = 0; k < 10; k++)
			{
				petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f + k * 0.1f);
				float thisScale = Helper.SwoopEase(Math.Clamp((scale - 0.3f) / 0.7f, 0, 1));
				float rot = baseRot + k / 10f * 6.28f + (k % 2 == 0 ? 0.1f : -0.1f) - 0.17f;
				spriteBatch.Draw(Assets.Bosses.BrainRedux.Frond.Value, pos + Vector2.UnitX.RotatedBy(rot) * 42 * thisScale * NPC.scale, null, Color.White, rot, bigOrigin, thisScale * NPC.scale * new Vector2(1, 0.5f), 0, 0);
			}

			petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.PetalSmall.Size());
			petalShader.Parameters["u_color"].SetValue(new Vector3(0.4f, 0.6f, 0.4f));
			petalShader.Parameters["u_fade"].SetValue(new Vector3(0.1f, 0.2f, 0.4f));

			for (int k = 0; k < 5; k++)
			{
				petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * -0.01f + k * 0.2f);
				float thisScale = Helper.SwoopEase(Math.Clamp((scale - 0.15f) / 0.7f, 0, 1));
				float finalScale = thisScale + (float)Math.Sin(Main.GameUpdateCount * 0.1f + k * 0.25f) * 0.025f;
				float rot = baseRot + k / 5f * 6.28f;
				spriteBatch.Draw(smallPetal, pos + Vector2.UnitX.RotatedBy(rot) * 48 * thisScale * NPC.scale, null, Color.White, rot, smallOrigin, finalScale * NPC.scale, 0, 0);
			}

			petalShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.PetalBig.Size());
			petalShader.Parameters["u_color"].SetValue(new Vector3(0.7f, 0.3f, 0.3f));
			petalShader.Parameters["u_fade"].SetValue(new Vector3(0.3f, 0.5f, 0.3f));

			for (int k = 0; k < 5; k++)
			{
				petalShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f + k * 0.2f);
				float thisScale = Helper.SwoopEase(Math.Clamp(scale / 0.7f, 0, 1));
				float finalScale = thisScale + (float)Math.Sin(Main.GameUpdateCount * 0.1f + k * -0.25f) * 0.05f;
				float rot = baseRot + k / 5f * 6.28f + 6.28f / 10f;
				spriteBatch.Draw(bigPetal, pos + Vector2.UnitX.RotatedBy(rot) * 32 * thisScale * NPC.scale, null, Color.White, rot, bigOrigin, finalScale * NPC.scale, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(glow, pos, null, Color.Black * (0.5f + 0.4f * scale), NPC.rotation, glow.Size() / 2f, NPC.scale * 2.8f, 0, 0);

			bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.FlowerCore.Size());
			bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

			bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.BrainRedux.FlowerCore.Value);
			bodyShader.Parameters["linemap_t"].SetValue(Assets.GUI.RingGlow.Value);
			bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
			bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.BrainRedux.HeartOver.Value);
			bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.BrainRedux.FlowerCoreNormal.Value);
			bodyShader.Parameters["u_color"].SetValue(new Vector3(0.9f, 0.7f, 0.2f) * Math.Min(scale / 0.2f, 1));
			bodyShader.Parameters["u_fade"].SetValue(new Vector3(0.0f, 0.2f, 0.4f));
			bodyShader.Parameters["mask_t"].SetValue(Assets.MagicPixel.Value);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, rasterizer, bodyShader, Main.GameViewMatrix.TransformationMatrix);

			Texture2D coreTex = Assets.Bosses.BrainRedux.FlowerCore.Value;
			spriteBatch.Draw(coreTex, pos, null, Color.White, NPC.rotation, coreTex.Size() / 2f, NPC.scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>
		/// Used in the death animation only
		/// </summary>
		/// <param name="spriteBatch"></param>
		private void DrawDeadFlower(SpriteBatch spriteBatch)
		{
			Vector2 pos = NPC.Center - Main.screenPosition;

			Texture2D glow = Assets.Keys.Glow.Value;
			spriteBatch.Draw(glow, pos, null, Color.Black * 0.5f, NPC.rotation, glow.Size() / 2f, NPC.scale * 5.5f, 0, 0);

			Texture2D bigPetal = Assets.Bosses.BrainRedux.PetalBig.Value;
			Texture2D smallPetal = Assets.Bosses.BrainRedux.PetalSmall.Value;

			var bigOrigin = new Vector2(0, bigPetal.Height / 2f);
			var smallOrigin = new Vector2(0, smallPetal.Height / 2f);
			float baseRot = flowerRotationOnDeath;

			for (int k = 0; k < 10; k++)
			{
				float rot = baseRot + k / 10f * 6.28f + (k % 2 == 0 ? 0.1f : -0.1f) - 0.17f;
				spriteBatch.Draw(Assets.Bosses.BrainRedux.Frond.Value, pos + Vector2.UnitX.RotatedBy(rot) * 42 * NPC.scale, null, Color.White, rot, bigOrigin, NPC.scale * new Vector2(1, 0.5f), 0, 0);
			}

			for (int k = 0; k < 5; k++)
			{
				float rot = baseRot + k / 5f * 6.28f;
				spriteBatch.Draw(smallPetal, pos + Vector2.UnitX.RotatedBy(rot) * 48 * NPC.scale, null, Color.White, rot, smallOrigin, NPC.scale, 0, 0);
			}

			for (int k = 0; k < 5; k++)
			{
				float rot = baseRot + k / 5f * 6.28f + 6.28f / 10f;
				spriteBatch.Draw(bigPetal, pos + Vector2.UnitX.RotatedBy(rot) * 32 * NPC.scale, null, Color.White, rot, bigOrigin, NPC.scale, 0, 0);
			}

			spriteBatch.Draw(glow, pos, null, Color.Black * 0.9f, NPC.rotation, glow.Size() / 2f, NPC.scale * 2.8f, 0, 0);

			Texture2D coreTex = Assets.Bosses.BrainRedux.FlowerCore.Value;
			spriteBatch.Draw(coreTex, pos, null, Color.White, NPC.rotation, coreTex.Size() / 2f, NPC.scale, 0, 0);
		}

		private void DrawFlowerToDead(SpriteBatch spriteBatch, float progress)
		{
			if (progress < 75)
			{
				DrawFlower(spriteBatch, 1, Main.screenPosition);
			}
			else
			{
				if (progress == 75)
					flowerRotationOnDeath = Main.GameUpdateCount * 0.01f;

				DrawDeadFlower(spriteBatch);
			}

			if (progress > 60 && progress < 90)
			{
				float flashTime = (progress - 60) / 30f;

				Texture2D glow = Assets.Keys.Glow.Value;
				Texture2D star = Assets.StarTexture.Value;

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.LinearWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, null, new Color(255, 220, 200, 255) * MathF.Sin(flashTime * 3.14f), 0, glow.Size() / 2f, flashTime * 25, 0, 0);
				spriteBatch.Draw(star, NPC.Center - Main.screenPosition, null, new Color(255, 255, 220, 0) * MathF.Sin(flashTime * 3.14f), 0, star.Size() / 2f, flashTime * new Vector2(6, 35), 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}

			if (progress == 299)
			{
				float baseRot = flowerRotationOnDeath;

				Texture2D bigPetal = Assets.Bosses.BrainRedux.PetalBig.Value;
				Texture2D smallPetal = Assets.Bosses.BrainRedux.PetalSmall.Value;
				Texture2D frond = Assets.Bosses.BrainRedux.Frond.Value;

				var bigOrigin = new Vector2(0, bigPetal.Height / 2f);
				var smallOrigin = new Vector2(0, smallPetal.Height / 2f);

				for (int k = 0; k < 10; k++) // Frond
				{
					float rot = baseRot + k / 10f * 6.28f + (k % 2 == 0 ? 0.1f : -0.1f) - 0.17f;
					var gore = Gore.NewGorePerfect(null, NPC.Center - frond.Size() / 2f + Vector2.UnitX.RotatedBy(rot) * (32 + frond.Width / 2) * NPC.scale, Vector2.UnitX.RotatedBy(rot) * 0.9f, StarlightRiver.Instance.Find<ModGore>("Frond").Type);
					gore.rotation = rot;
				}

				for (int k = 0; k < 5; k++) // Small
				{
					float rot = baseRot + k / 5f * 6.28f;
					var gore = Gore.NewGorePerfect(null, NPC.Center - smallPetal.Size() / 2f + Vector2.UnitX.RotatedBy(rot) * (48 + smallPetal.Width / 2) * NPC.scale, Vector2.UnitX.RotatedBy(rot) * 0.7f, StarlightRiver.Instance.Find<ModGore>("PetalSmall").Type);
					gore.rotation = rot;
				}

				for (int k = 0; k < 5; k++) // Large
				{
					float rot = baseRot + k / 5f * 6.28f + 6.28f / 10f;
					var gore = Gore.NewGorePerfect(null, NPC.Center - bigPetal.Size() / 2f + Vector2.UnitX.RotatedBy(rot) * (32 + bigPetal.Width / 2) * NPC.scale, Vector2.UnitX.RotatedBy(rot) * 0.5f, StarlightRiver.Instance.Find<ModGore>("PetalBig").Type);
					gore.rotation = rot;
				}
			}
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
				NPC.scale = 1f - Helper.BezierEase((time - 190) / 70f) * 0.4f;

			if (time > 500 && time < 550)
				NPC.scale = 0.6f + Helper.BezierEase((time - 500) / 50f) * 0.4f;

			DrawHeartToFlower(sb, timeToPass, screenPos);
		}
	}
}