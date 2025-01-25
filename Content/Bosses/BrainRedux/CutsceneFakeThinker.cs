using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class CutsceneFakeThinker : ModNPC
	{
		public static Effect bodyShader;
		public Vector2 homePos;

		public static CutsceneFakeThinker ThisThinker => Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<CutsceneFakeThinker>())?.ModNPC as CutsceneFakeThinker;

		public ref float GrayAuraRadius => ref NPC.ai[0];
		public ref float Timer => ref NPC.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawGrayAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawShadedBody;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.lifeMax = 100;
			NPC.damage = 1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.width = 100;
			NPC.height = 100;

			NPC.dontTakeDamage = true;
			NPC.immortal = true;
		}

		public override void SetBestiary(Terraria.GameContent.Bestiary.BestiaryDatabase database, Terraria.GameContent.Bestiary.BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

		public override void AI()
		{
			if (homePos == default)
				homePos = NPC.Center;

			Timer++;

			Lighting.AddLight(NPC.Center, new Vector3(1f, 0.9f, 0.8f));

			if (Timer < 60)
			{
				NPC.scale = Math.Min(Timer / 60f, 1f);
				GrayAuraRadius = Math.Min(Timer / 60f * 80f, 80f);
			}

			if (Timer == 1)
			{
				foreach (Player player in Main.player.Where(n => Vector2.Distance(n.Center, NPC.Center) < 4000))
					player.ActivateCutscene<ThinkerOpenCutscene>();

				if (Main.LocalPlayer.InCutscene<ThinkerOpenCutscene>())
				{
					CameraSystem.MoveCameraOut(30, homePos + Vector2.UnitY * -100);
					ZoomHandler.SetZoomAnimation(1.5f, 30);
				}

				for (int k = 0; k < 50; k++)
				{
					var color = new Color(Main.rand.NextFloat(0.2f, 1f), Main.rand.NextFloat(0.2f, 1f), Main.rand.NextFloat(0.2f, 1f));
					float rot = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * 20, ModContent.DustType<Dusts.Cinder>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(3), 0, color, Main.rand.NextFloat(1f, 3f));
				}
			}

			if (Timer > 30 && Timer <= 90)
			{
				NPC.Center = Vector2.Lerp(homePos, homePos + Vector2.UnitY * -200, Helpers.Helper.SwoopEase((Timer - 30) / 60f));
			}

			if (Timer == 75)
				Helpers.Helper.PlayPitched("Magic/FireCast", 1, -0.5f, NPC.Center);

			if (Timer > 90 && Timer < 120)
			{
				float prog = (Timer - 90) / 30f;
				NPC.scale = 1f + (float)Math.Sin(prog * 3.14f) * 0.25f;

				GrayAuraRadius += 2f;

				for (int k = 0; k < 5; k++)
				{
					var color = new Color(Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.5f, 1f));
					float rot = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * 45, ModContent.DustType<Dusts.GlowLineFast>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(8), 0, color, Main.rand.NextFloat(0.5f, 1f));
					Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * GrayAuraRadius / 2f, ModContent.DustType<Dusts.GraymatterDust>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(8), 0, color, Main.rand.NextFloat(0.5f, 2f));
				}
			}

			if (Timer > 130 && GrayAuraRadius > 10)
			{
				float rot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * GrayAuraRadius / 2f, ModContent.DustType<Dusts.GraymatterDust>(), Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(1), 0, Color.White, Main.rand.NextFloat(0.5f, 1f));
			}

			if (Timer == 130)
			{
				if (Vector2.Distance(Main.LocalPlayer.Center, homePos) < 3000)
					ZoomHandler.SetZoomAnimation(1f, 90);
			}

			if (Timer > 180 && Timer < 240)
			{
				NPC.velocity.Y -= 0.25f;
			}

			// Calculates how long we need to show every thinker spawn position
			int locationsToShow = ModContent.GetInstance<GraymatterBiomeSystem>().thinkerPositions.Count;
			int timePerLocation = 270;
			int durationToShowAll = locationsToShow * timePerLocation;

			if (Timer >= 240 && Timer < 240 + durationToShowAll)
			{
				// Automatically generates a timer for each "visit" to the thinker spawn positions
				float currTime = (Timer - 240) % timePerLocation;
				int locationIndex = (int)((Timer - 240) / timePerLocation);

				Vector2 location = ModContent.GetInstance<GraymatterBiomeSystem>().thinkerPositions[locationIndex] * 16;

				// Camera FX only if the local player is in the cutscene
				if (Main.LocalPlayer.InCutscene<ThinkerOpenCutscene>())
				{
					if (currTime < 30)
					{
						Fadeout.color = Color.Black;
						Fadeout.opacity = currTime / 30f;
					}

					if (currTime == 30)
					{
						CameraSystem.TeleportCamera(location);
						GrayAuraRadius = 140;
					}

					if (currTime > 30 && currTime < 90)
					{
						Fadeout.color = Color.Black;
						Fadeout.opacity = 1f - (currTime - 30) / 60f;
					}
				}

				if (currTime == 60)
				{
					NPC.Center = location + new Vector2(0, -800);
					NPC.velocity *= 0;
				}

				if (currTime > 60 && currTime < 150)
				{
					NPC.Center = Vector2.Lerp(location + new Vector2(0, -800), location, Helper.SwoopEase((currTime - 60) / 90f));
				}

				if (currTime > 180 && currTime < 210)
				{
					GrayAuraRadius = 140 * (1f - (currTime - 180) / 30f);
				}
			}

			// Camera FX only if the local player is in the cutscene. returning
			if (Main.LocalPlayer.InCutscene<ThinkerOpenCutscene>())
			{
				if (Timer > 240 + durationToShowAll)
				{
					Fadeout.color = Color.Black;
					Fadeout.opacity = (Timer - (240 + durationToShowAll)) / 30f;
				}

				if (Timer == 270 + durationToShowAll)
				{
					StarlightWorld.Flag(WorldFlags.ThinkerBossOpen);
					CameraSystem.TeleportCameraBack();
				}

				if (Timer > 270 + durationToShowAll)
				{
					Fadeout.color = Color.Black;
					Fadeout.opacity = 1f - (Timer - (270 + durationToShowAll)) / 30f;
				}
			}

			if (Timer == 300 + durationToShowAll)
			{
				StarlightWorld.Flag(WorldFlags.ThinkerBossOpen);

				if (Main.LocalPlayer.InCutscene<ThinkerOpenCutscene>())
					Main.LocalPlayer.DeactivateCutscene();

				Main.NewText("The thinkers are thinking...", Color.Yellow);

				NPC.active = false;
			}
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			float shellOpacity = 0.01f;

			if (Timer > 240)
				shellOpacity = 0.01f + 1f - GrayAuraRadius / 140f;

			Texture2D tex = Assets.Bosses.BrainRedux.ShellBack.Value;
			Vector2 pos = NPC.Center - Main.screenPosition - tex.Size() / 2f;
			Color color = Color.White;
			color.A = (byte)(shellOpacity * 255);

			LightingBufferRenderer.DrawWithLighting(pos, tex, color * shellOpacity);

			return false;
		}

		private void DrawGrayAura(SpriteBatch batch)
		{
			if (ThisThinker is null)
				return;

			Texture2D glow = Assets.Keys.GlowAlpha.Value;
			Color color = Color.White;
			color.A = 0;

			for (int k = 0; k < 8; k++)
			{
				batch.Draw(glow, ThisThinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, ThisThinker.GrayAuraRadius * 4 / glow.Width, 0, 0);
			}
		}

		private void DrawShadedBody(SpriteBatch sb)
		{
			if (ThisThinker is null)
				return;

			bodyShader ??= Filters.Scene["ThinkerBody"].GetShader().Shader;

			Texture2D glow = Assets.Keys.Glow.Value;

			CutsceneFakeThinker thinker = ThisThinker;

			sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, Color.Black * 0.5f, thinker.NPC.rotation, glow.Size() / 2f, thinker.NPC.scale * 2.5f, 0, 0);

			bodyShader.Parameters["u_resolution"].SetValue(Assets.Bosses.BrainRedux.Heart.Size());
			bodyShader.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f);

			bodyShader.Parameters["mainbody_t"].SetValue(Assets.Bosses.BrainRedux.Heart.Value);
			bodyShader.Parameters["linemap_t"].SetValue(Assets.Bosses.BrainRedux.HeartLine.Value);
			bodyShader.Parameters["noisemap_t"].SetValue(Assets.Noise.ShaderNoise.Value);
			bodyShader.Parameters["overlay_t"].SetValue(Assets.Bosses.BrainRedux.HeartOver.Value);
			bodyShader.Parameters["normal_t"].SetValue(Assets.Bosses.BrainRedux.HeartNormal.Value);

			sb.End();
			sb.Begin(default, default, SamplerState.PointWrap, default, default, bodyShader, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = Assets.Bosses.BrainRedux.Heart.Value;
			sb.Draw(tex, thinker.NPC.Center - Main.screenPosition, null, Color.White, thinker.NPC.rotation, tex.Size() / 2f, thinker.NPC.scale, 0, 0);

			sb.End();
			sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}