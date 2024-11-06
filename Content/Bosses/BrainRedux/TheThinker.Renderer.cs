using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class TheThinker
	{
		private List<Vector2> arenaCache;
		private Trail arenaTrail;

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}

		public void DrawUnderShell()
		{
			Texture2D tex = Assets.Bosses.BrainRedux.ShellBack.Value;
			Vector2 pos = home - Main.screenPosition - tex.Size() / 2f;

			LightingBufferRenderer.DrawWithLighting(pos, tex);
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
				Vector2 edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 70 + offset);
				spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 60, 0) * 0.25f * DeadBrain.ArenaOpacity, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
				spriteBatch.Draw(solid, edge - Main.screenPosition, new Rectangle(0, 0, 58, 60), new Color(Lighting.GetSubLight(edge)) * DeadBrain.ArenaOpacity, rot - 1.57f / 2f, new Vector2(58, 60), 1f, 0, 0);

				offset = (float)Math.Sin((k + 0.5) / 36f * 6.28f * 8 + Main.GameUpdateCount * 0.025f) * 15;
				offset += (float)Math.Cos((k + 0.5) / 36f * 6.28f * 6 + Main.GameUpdateCount * 0.04f) * 15;
				rot = (k + 0.5f) / 36f * 6.28f;
				float sin = (float)Math.Sin(Main.GameUpdateCount * 0.01f + k);
				edge = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 80 + sin * 10 + offset);
				spriteBatch.Draw(spike, edge - Main.screenPosition, new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height), new Color(255, 50, 60, 0) * 0.25f * (1f - sin + 0.5f) * DeadBrain.ArenaOpacity, rot - 1.57f, new Vector2(spike.Width / 4f, spike.Height), 1.5f, 0, 0);
				spriteBatch.Draw(solid, edge - Main.screenPosition, new Rectangle(58, 0, 58, 60), new Color(Lighting.GetSubLight(edge)) * DeadBrain.ArenaOpacity, rot - 1.57f / 2f, new Vector2(58, 60), 1f, 0, 0);
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

				arenaCache[k] = home + Vector2.UnitX.RotatedBy(rot) * (hurtRadius + 70 + offset);
			}

			arenaCache[72] = arenaCache[0];

			if (arenaTrail is null || arenaTrail.IsDisposed)
			{
				arenaTrail = new Trail(Main.instance.GraphicsDevice, 73, new NoTip(), factor => 56, factor =>
				{
					int index = (int)(factor.X * 72);
					return new Color(Lighting.GetSubLight(arenaCache[index])) * DeadBrain.ArenaOpacity;
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
			DrawUnderShell();

			if (active)
				DrawArena(spriteBatch);

			return false;
		}

		private void DrawGrayAura(SpriteBatch sb)
		{
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			Color color = Color.White;
			color.A = 0;

			foreach (TheThinker thinker in toRender)
			{
				for (int k = 0; k < 8; k++)
				{
					sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, (140 + thinker.ExtraRadius) * 4 / glow.Width, 0, 0);
				}
			}

			toRender.RemoveAll(n => n is null || !n.NPC.active);
		}

		private void DrawShadedBody(SpriteBatch sb)
		{
			bodyShader ??= Terraria.Graphics.Effects.Filters.Scene["ThinkerBody"].GetShader().Shader;

			foreach (TheThinker thinker in toRender)
			{
				thinker.DrawUnderShell();

				bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.Heart.Size());
				bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

				bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.BrainRedux.Heart.Value);
				bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.BrainRedux.HeartLine.Value);
				bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.PerlinNoise.Value);
				bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.BrainRedux.HeartOver.Value);
				bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.BrainRedux.HeartNormal.Value);

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default, bodyShader, Main.GameViewMatrix.TransformationMatrix);

				Texture2D tex = Assets.Bosses.BrainRedux.Heart.Value;
				sb.Draw(tex, thinker.NPC.Center - Main.screenPosition, null, Color.White, thinker.NPC.rotation, tex.Size() / 2f, thinker.NPC.scale, 0, 0);

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}
}
