using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CameraSystem;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Starlight
{
	class Crow : ModNPC
	{
		public bool visible;

		public override string Texture => "StarlightRiver/Assets/NPCs/Starlight/Crow";

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float CutsceneTimer => ref NPC.ai[2];
		public ref float TextState => ref NPC.ai[3];

		public bool InCutscene
		{
			get => State == 1;
			set => State = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("???");
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 40;
			NPC.height = 64;
			NPC.aiStyle = -1;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 400;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0;
			NPC.gfxOffY = -4;

			visible = false;
		}

		public override void AI()
		{
			Timer++;

			if (InCutscene)
				CutsceneTimer++;

			if (Main.LocalPlayer.controlHook)
			{
				CutsceneTimer = 0;
				visible = false;
			}

			if (visible)
			{
				if (Main.rand.NextBool(6))
					Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-16, 16), 32), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.25f), 0, new Color(100, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.35f, 0.45f));

				if (Timer % 120 == 0)
					Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-32, 32), 24), ModContent.DustType<Dusts.VerticalGlow>(), Vector2.Zero, 0, new Color(40, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.9f, 1.1f));

				Lighting.AddLight(NPC.Center, new Vector3(0.1f, 0.2f, 0.25f) * 4);
			}

			if (!InCutscene && Main.player.Any(n => n.active && Vector2.Distance(n.Center, NPC.Center) < 400))
			{
				CutsceneTimer = 0;
				InCutscene = true;
			}

			if (InCutscene && (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)) //handles cutscenes
			{
				Player player = Main.LocalPlayer;

				switch (StarlightEventSequenceSystem.sequence)
				{
					case 0:
						FirstEncounter();
						break;

					case 1:
						SecondEncounter();
						break;
				}
			}
		}

		/// <summary>
		/// The NPCs spawning animation, which is re-used between encounters
		/// </summary>
		private void SpawnAnimation()
		{
			if (CutsceneTimer > 1 && CutsceneTimer < 30 && Main.rand.NextBool())
			{
				int type = ModContent.DustType<Dusts.AuroraFast>();
				float rot = Main.rand.NextFloat(6.28f);
				var d = Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedBy(rot) * 160, type, Vector2.One.RotatedBy(rot) * -Main.rand.NextFloat(16, 18), 0, new Color(40, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.5f, 1f));
				d.customData = Main.rand.NextFloat(0.8f, 1.5f);
			}

			if (CutsceneTimer >= 130 && CutsceneTimer <= 140)
			{
				for (int K = 0; K < 2; K++)
				{
					int type = ModContent.DustType<Dusts.Cinder>();

					if (Main.rand.NextBool(4))
						type = ModContent.DustType<Dusts.Aurora>();

					var d = Dust.NewDustPerfect(NPC.Center, type, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, new Color(40, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.5f, 1f));
					d.customData = Main.rand.NextFloat(0.5f, 1f);
				}
			}

			if (CutsceneTimer > 100 && CutsceneTimer < 140)
			{
				Lighting.AddLight(NPC.Center, new Vector3(0.1f, 0.2f, 0.25f) * (CutsceneTimer - 100) / 40f * 4);
			}

			if (CutsceneTimer == 140)
			{
				visible = true;

				for (int k = 0; k < 20; k++)
				{

				}
			}
		}

		/// <summary>
		/// Rendering related to drawing a flashing star, for various animations. Takes 140 frames to complete
		/// </summary>
		private void DrawFlashingStar(SpriteBatch spriteBatch, float timer)
		{
			Texture2D star = ModContent.Request<Texture2D>("StarlightRiver/Assets/StarTexture").Value;
			Vector2 pos = NPC.Center - Main.screenPosition;

			// Fade in
			if (timer > 40 && timer < 120)
			{
				float prog = (timer - 40) / 80f;
				float opacity = Helpers.Helper.BezierEase(prog);

				Color starColor = new Color(150, 220, 255) * Helpers.Helper.SwoopEase(prog);
				starColor.A = 0;

				Color whiteColor = Color.White * Helpers.Helper.SwoopEase(prog);
				whiteColor.A = 0;

				spriteBatch.Draw(star, pos, star.Frame(), starColor, 0, star.Size() / 2f, Helpers.Helper.SwoopEase(prog) * 1.5f, SpriteEffects.None, 0);
				spriteBatch.Draw(star, pos, star.Frame(), whiteColor, 0, star.Size() / 2f, Helpers.Helper.SwoopEase(prog), SpriteEffects.None, 0);
			}

			// Hold
			if (timer >= 120 && timer <= 140)
			{
				float colorProg = Helpers.Helper.BezierEase((timer - 120) / 20f);
				var color = Color.Lerp(new Color(50, 120, 255), Color.White, colorProg);

				Color starColor = Color.Lerp(new Color(150, 220, 255), Color.White, colorProg) * (1 - colorProg);
				starColor.A = 0;

				Color whiteColor = Color.White * (1 - colorProg);
				whiteColor.A = 0;

				spriteBatch.Draw(star, pos, star.Frame(), starColor, 0, star.Size() / 2f, (1 + colorProg) * 1.5f, SpriteEffects.None, 0);
				spriteBatch.Draw(star, pos, star.Frame(), whiteColor, 0, star.Size() / 2f, 1 + colorProg, SpriteEffects.None, 0);
			}
		}

		/// <summary>
		/// The NPCs leaving animation, which is re-used between encounters. Note this will increment the starlight event sequence.
		/// </summary>
		/// <param name="timer"></param>
		private void LeaveAnimation(float timer)
		{
			if (timer > 120)
			{
				NPC.active = false;
				StarlightEventSequenceSystem.willOccur = false;
				StarlightEventSequenceSystem.occuring = false;

				StarlightEventSequenceSystem.sequence++;
			}
		}

		/// <summary>
		/// Dictates the NPCs behavior during the first encounter, where the player is given stamina and the hint ability
		/// </summary>
		private void FirstEncounter()
		{
			Main.LocalPlayer.GetHandler().Stamina = 0;
			Main.LocalPlayer.GetHandler().SetStaminaRegenCD(0);

			if (CutsceneTimer == 1)
				CameraSystem.MoveCameraOut(30, NPC.Center, Vector2.SmoothStep);

			if (CutsceneTimer < 300)
				SpawnAnimation();

			if (CutsceneTimer == 360) // First encounter
			{
				RichTextBox.OpenDialogue(NPC, "Crow?", GetIntroDialogue());
				RichTextBox.AddButton("Tell me more", () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, "Crow?", GetIntroDialogue());

					if (TextState == 2)
					{
						RichTextBox.ClearButtons();
						RichTextBox.AddButton("Accept", () =>
						{
							Main.LocalPlayer.GetHandler().Unlock<HintAbility>();
							Stamina.gainAnimationTimer = 240;

							TextState++;
							RichTextBox.SetData(NPC, "Crow?", GetIntroDialogue());

							RichTextBox.ClearButtons();
							RichTextBox.AddButton("Accept", () =>
							{
								CameraSystem.ReturnCamera(30, Vector2.SmoothStep);
								RichTextBox.CloseDialogue();
								CutsceneTimer = 363;

								string message = StarlightRiver.Instance.AbilityKeys.Get<HintAbility>().GetAssignedKeys().Count > 0 ?
									$"Aim your cursor and press {StarlightRiver.Instance.AbilityKeys.Get<HintAbility>().GetAssignedKeys()[0]} to inspect the world." :
									"Aim your cursor and press [Please bind a key] to inspect the world.";

								Main.LocalPlayer.GetHandler().GetAbility(out HintAbility hint);
								UILoader.GetUIState<TextCard>().Display("Starsight", message, hint);
							});
						});
					}
				});
			}

			if (CutsceneTimer == 362)
				CutsceneTimer = 361;

			if (CutsceneTimer >= 362)
			{
				LeaveAnimation(CutsceneTimer - 362);
			}
		}

		/// <summary>
		/// Dictates the NPCs behavior during the second encounter, where the player recieves an infusion slot
		/// </summary>
		private void SecondEncounter()
		{
			if (CutsceneTimer == 1)
				CameraSystem.MoveCameraOut(30, NPC.Center, Vector2.SmoothStep);

			if (CutsceneTimer < 300)
				SpawnAnimation();

			if (CutsceneTimer == 360) // First encounter
			{
				RichTextBox.OpenDialogue(NPC, "Crow?", GetInfusionDialogue());
				RichTextBox.AddButton("Tell me more", () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, "Crow?", GetInfusionDialogue());

					RichTextBox.ClearButtons();
					RichTextBox.AddButton("Accept", () =>
					{
						Main.LocalPlayer.GetHandler().InfusionLimit++;

						TextState++;
						RichTextBox.SetData(NPC, "Crow?", GetInfusionDialogue());

						RichTextBox.ClearButtons();
						RichTextBox.AddButton("Goodbye", () =>
						{
							CameraSystem.ReturnCamera(30, Vector2.SmoothStep);
							RichTextBox.CloseDialogue();
							CutsceneTimer = 363;
						});
					});

				});
			}

			if (CutsceneTimer == 362)
				CutsceneTimer = 361;

			if (CutsceneTimer >= 362)
			{
				Main.playerInventory = true;
			}

			if (CutsceneTimer == 380)
				Infusion.gainAnimationTimer = 240;

			if (CutsceneTimer >= 500)
			{
				LeaveAnimation(CutsceneTimer - 500);
			}
		}

		private string GetIntroDialogue()
		{
			return TextState switch
			{
				0 => "Greetings wanderer. I have been waiting for a very long time to meet you.",
				1 => "This is not what I will actually say to you, this is a placeholder",
				2 => "I will now give you the magical power of STARLIGHT! It will let you write better dialogue for me in the future or something.",
				3 => "Next you will get the tutorial for the hint ability after you hit next, since this is just a placeholder",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		private string GetInfusionDialogue()
		{
			return TextState switch
			{
				0 => "Placeholder 1",
				1 => "Placeholder 2",
				2 => "Placeholder 3",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (InCutscene && CutsceneTimer < 140)
				DrawFlashingStar(spriteBatch, CutsceneTimer);

			return visible;
		}
	}
}