using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.NPCs.Starlight;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Events
{
	internal class StarlightEventSequenceSystem : ModSystem
	{
		public static int sequence = 0;
		public static bool willOccur = false;
		public static bool occuring = false;

		public static int fadeTimer;

		public static bool Active => !Main.dayTime && (occuring || fadeTimer > 0);

		public override void PostUpdateTime()
		{
			// Handles the fade effects
			if (occuring && fadeTimer < 300)
				fadeTimer++;

			if (!occuring && fadeTimer > 0)
				fadeTimer--;

			if (Main.dayTime && occuring)
			{
				occuring = false;
				willOccur = true;
			}

			// The event should trigger the next applicable night
			if (willOccur && !Main.dayTime && Main.time == 0)
			{
				occuring = true;
				willOccur = false;
				Main.NewText("A strange traveler has arrived...", new Color(150, 200, 255));
				NPC.NewNPC(null, Main.spawnTileX * 16, Main.spawnTileY * 16 - 120, ModContent.NPCType<Crow>());
			}

			if (Active)
			{
				Main.bloodMoon = false;

				if (Main.time == 600)
				{
					int i = Projectile.NewProjectile(null, Main.LocalPlayer.Center + new Vector2(0, -48), Vector2.Zero, ModContent.ProjectileType<HintText>(), 0, 0, Main.myPlayer);
					var proj = Main.projectile[i].ModProjectile as HintText;

					if (proj != null)
					{
						proj.text = "Wanderer, I await you at the place you entered this strange world...";
						proj.follow = true;
						proj.Projectile.scale = 1.2f;
					}
				}

				if (Main.time == 8000)
				{
					int i = Projectile.NewProjectile(null, Main.LocalPlayer.Center + new Vector2(0, -48), Vector2.Zero, ModContent.ProjectileType<HintText>(), 0, 0, Main.myPlayer);
					var proj = Main.projectile[i].ModProjectile as HintText;

					if (proj != null)
					{
						proj.text = "Wanderer, do not keep me waiting...";
						proj.follow = true;
						proj.Projectile.scale = 1.2f;
					}
				}

				if (Main.time == 16200)
				{
					int i = Projectile.NewProjectile(null, Main.LocalPlayer.Center + new Vector2(0, -48), Vector2.Zero, ModContent.ProjectileType<HintText>(), 0, 0, Main.myPlayer);
					var proj = Main.projectile[i].ModProjectile as HintText;

					if (proj != null)
					{
						proj.text = "Wanderer, I brought a gift... and ice cream...";
						proj.follow = true;
						proj.Projectile.scale = 1.2f;
					}
				}
			}

			if (Active && Main.time > 32400 / 2)
				Main.time = 32400 / 2;
		}

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
			if (Active)
			{
				tileColor = Color.Lerp(tileColor, new Color(2, 50, 62), fadeTimer / 300f);
				backgroundColor = Color.Lerp(backgroundColor, new Color(2, 50, 62), fadeTimer / 300f);
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["sequence"] = sequence;
			tag["willOccur"] = willOccur | occuring;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			sequence = tag.GetInt("sequence");
			willOccur = tag.GetBool("willOccur");

			if (!Main.dayTime && willOccur)
			{
				occuring = true;
				willOccur = false;
			}
		}
	}

	internal class StarlightEvent : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/Moonstone");

		public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

		public override void Load()
		{
			base.Load();
			StarlightNPC.OnKillEvent += TriggerEventActivation;
			On_Main.DrawStarsInBackground += DrawRiver;
			StarlightRiverBackground.CheckIsActiveEvent += () => IsSceneEffectActive(Main.LocalPlayer);
			StarlightRiverBackground.DrawMapEvent += DrawOverlayMap;
			StarlightRiverBackground.DrawOverlayEvent += DrawOverlay;
		}

		private void DrawOverlayMap(SpriteBatch sb)
		{
			if (IsSceneEffectActive(Main.LocalPlayer))
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/SwirlyNoiseLooping").Value;

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default);

				sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Rectangle((int)Main.GameUpdateCount / 3, 0, tex.Width, tex.Height), Color.White * (StarlightEventSequenceSystem.fadeTimer / 300f) * 0.5f);

				NPC crow = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Crow>());

				if (crow != null && crow.ModNPC != null)
				{
					var mn = crow.ModNPC as Crow;

					if (mn != null)
					{
						Texture2D glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowBlack").Value;

						float opacity = Math.Min(1f, mn.CutsceneTimer / 140f);
						Color color = Color.Black * opacity;
						//color.A = 0;

						sb.Draw(glowTex, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), null, color, 0, glowTex.Size() / 2f, opacity * 12f, 0, 0);
					}
				}
			}
		}

		private void DrawRiver(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
		{
			if (IsSceneEffectActive(Main.LocalPlayer))
				Main.spriteBatch.Draw(StarlightRiverBackground.starsTarget.RenderTarget, Vector2.Zero, Color.White * (StarlightEventSequenceSystem.fadeTimer / 300f));

			orig(self, sceneArea, artificial);
		}

		private void DrawOverlay(GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget)
		{
			if (IsSceneEffectActive(Main.LocalPlayer))
			{
				SpriteBatch spriteBatch = Main.spriteBatch;

				Effect mapEffect = Filters.Scene["StarMap"].GetShader().Shader;
				mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
				mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

				spriteBatch.Begin(default, default, default, default, default, mapEffect, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

				spriteBatch.End();

				spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/SwirlyNoiseLooping").Value;
				spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Rectangle((int)Main.GameUpdateCount / 3, 0, tex.Width, tex.Height), Color.Cyan * (StarlightEventSequenceSystem.fadeTimer / 300f) * 0.2f);

				spriteBatch.End();
			}
		}

		private void TriggerEventActivation(NPC NPC) //TODO: This might be worth moving elsewhere? This is a bit hidden away here
		{
			if (NPC.boss && StarlightEventSequenceSystem.sequence <= 0) //First visit is after any boss
				StarlightEventSequenceSystem.willOccur = true;

			if (NPC.type == ModContent.NPCType<VitricBoss>() && StarlightEventSequenceSystem.sequence == 1) //Second visit is after ceiros
				StarlightEventSequenceSystem.willOccur = true;
		}

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightEventSequenceSystem.Active;
		}
	}
}
