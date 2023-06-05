using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Starlight
{
	class Crow : ModNPC, IHintable
	{
		public bool visible;
		public bool leaving;

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
			NPC.noGravity = true;

			NPC.frame = new Rectangle(0, 0, 0, 0);

			visible = false;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void AI()
		{
			Timer++;

			if (InCutscene || leaving)
				CutsceneTimer++;

			if (visible)
			{
				if (Main.rand.NextBool(6))
					Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-16, 16), 32), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.25f), 0, new Color(100, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.35f, 0.45f));

				if (Timer % 120 == 0)
					Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-32, 32), 24), ModContent.DustType<Dusts.VerticalGlow>(), Vector2.Zero, 0, new Color(40, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.9f, 1.1f));

				Lighting.AddLight(NPC.Center, new Vector3(0.1f, 0.2f, 0.25f) * 4);
			}

			if (!InCutscene && !leaving && Main.player.Any(n => n.active && Vector2.Distance(n.Center, NPC.Center) < 400))
			{
				CutsceneTimer = 0;
				InCutscene = true;
			}

			if (InCutscene && (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)) //handles cutscenes
			{
				Player player = Main.LocalPlayer;
				player.immune = true; //TODO: Move this later!!!
				player.immuneTime = 2;

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

			if (leaving)
			{
				LeaveAnimation();
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
				for (int K = 0; K < 4; K++)
				{
					int type = ModContent.DustType<Dusts.Cinder>();

					if (Main.rand.NextBool(4))
						type = ModContent.DustType<Dusts.Aurora>();

					var d = Dust.NewDustPerfect(NPC.Center, type, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, new Color(40, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.5f, 1f));
					d.customData = Main.rand.NextFloat(0.5f, 1f);
				}
			}

			if (CutsceneTimer > 100 && CutsceneTimer < 140)
				Lighting.AddLight(NPC.Center, new Vector3(0.1f, 0.2f, 0.25f) * (CutsceneTimer - 100) / 40f * 4);

			if (CutsceneTimer == 140)
			{
				NPC.noGravity = false;
				visible = true;
			}

			if (CutsceneTimer >= 140 && CutsceneTimer < 160)
				SetFrame(0, (int)(CutsceneTimer - 140) / 10);

			if (CutsceneTimer > 160 && CutsceneTimer % 8 == 0 && NPC.velocity.Y == 0 && GetFrame().Y <= 11)
				SetFrame(0, GetFrame().Y + 1);
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
		private void LeaveAnimation()
		{
			leaving = true;

			if (CutsceneTimer >= 60)
				visible = false;

			if (CutsceneTimer == 130)
			{
				for (int k = 0; k < 40; k++)
				{
					float rand = Main.rand.NextFloat(-16, 16);
					float yVel = (1 - Math.Abs(rand) / 16) * Main.rand.Next(5, 25) * (Main.rand.NextBool() ? -1 : 1);

					var d = Dust.NewDustPerfect(NPC.Center + Vector2.UnitX * rand, ModContent.DustType<Dusts.Aurora>(), Vector2.UnitY * yVel, 0, new Color(100, Main.rand.Next(150, 255), 255), 1);
					d.customData = Main.rand.NextFloat(1f);
				}
			}

			if (CutsceneTimer >= 140)
			{
				NPC.active = false;
				StarlightEventSequenceSystem.willOccur = false;
				StarlightEventSequenceSystem.occuring = false;

				StarlightEventSequenceSystem.sequence++;
			}
		}

		/// <summary>
		/// Helper to trigger the leaving state
		/// </summary>
		private void Leave()
		{
			InCutscene = false;
			CutsceneTimer = 0;

			leaving = true;
		}

		/// <summary>
		/// Dictates the NPCs behavior during the first encounter, where the player is given stamina and the hint ability
		/// </summary>
		private void FirstEncounter()
		{
			Main.LocalPlayer.GetHandler().Stamina = 0;
			Main.LocalPlayer.GetHandler().SetStaminaRegenCD(0);

			if (CutsceneTimer == 1)
				CameraSystem.MoveCameraOut(30, NPC.Center + Vector2.UnitY * 120, Vector2.SmoothStep);

			if (CutsceneTimer < 300)
				SpawnAnimation();
			else
				NPC.direction = Main.LocalPlayer.Center.X > NPC.Center.X ? 1 : -1;

			if (CutsceneTimer == 360) // First encounter
			{
				if (Main.LocalPlayer.GetHandler().Unlocked<HintAbility>()) // If they already have the ability, special abort dialogue
				{
					RichTextBox.OpenDialogue(NPC, "Alican", "Oh, strange seeing you again here... Sorry, I thought you were someone else. I must leave to search for them now.");

					RichTextBox.ClearButtons();
					RichTextBox.AddButton("Bye!", () =>
					{
						CameraSystem.ReturnCamera(30, Vector2.SmoothStep);
						RichTextBox.CloseDialogue();
						CutsceneTimer = 363;
					});
					return;
				}

				RichTextBox.OpenDialogue(NPC, "Crow?", GetIntroDialogue());
				RichTextBox.AddButton("Continue", () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, "Crow?", GetIntroDialogue());

					if (TextState == 3)
						RichTextBox.SetData(NPC, "Alican", GetIntroDialogue());
					if (TextState == 4)
					{
						RichTextBox.ClearButtons();
						RichTextBox.AddButton("Accept", () =>
						{
							TextState++;
							RichTextBox.SetData(NPC, "Alican", GetIntroDialogue());
							if (TextState == 5)
							{
								RichTextBox.ClearButtons();
								RichTextBox.AddButton("Accept", () =>
								{
									Main.LocalPlayer.GetHandler().Unlock<HintAbility>();
									Stamina.gainAnimationTimer = 240;

									TextState++;
									RichTextBox.SetData(NPC, "Alican", GetIntroDialogue());

									RichTextBox.ClearButtons();
									RichTextBox.AddButton("Bye?", () =>
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
				});
			}

			if (CutsceneTimer == 362)
				CutsceneTimer = 361;

			if (CutsceneTimer >= 362)
				Leave();
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
				if (Main.LocalPlayer.GetHandler().InfusionLimit >= 1) // If they already have the infusion slot, special abort dialogue
				{
					RichTextBox.OpenDialogue(NPC, "Alican", "Oh, strange seeing you again here... Sorry, I thought you were someone else. I must leave to search for them now.");

					RichTextBox.ClearButtons();
					RichTextBox.AddButton("Bye!", () =>
					{
						CameraSystem.ReturnCamera(30, Vector2.SmoothStep);
						RichTextBox.CloseDialogue();
						CutsceneTimer = 363;
					});
					return;
				}

				RichTextBox.OpenDialogue(NPC, "Alican", GetInfusionDialogue());
				RichTextBox.AddButton("What?", () =>
				{
					TextState++;
					RichTextBox.SetData(NPC, "Alican", GetInfusionDialogue());

					RichTextBox.ClearButtons();
					RichTextBox.AddButton("Accept", () =>
					{
						Main.LocalPlayer.GetHandler().InfusionLimit++;

						TextState++;
						RichTextBox.SetData(NPC, "Alican", GetInfusionDialogue());

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
				Leave();
		}

		private string GetIntroDialogue()
		{
			return TextState switch
			{
				0 => "The crow-like... creature... gets up off the ground with a triumphant look in its beady eyes, dusting itself off, and then straightening its ruffled feathers.",
				1 => "\"There you are! I've jumped through seventeen different axons and the half the entire Capricorn Tropic trying to find you!\"",
				2 => "\"Yes, yes, my name is Alican, and I believe we can help each other. You see, I am a Seeker. Of what, exactly, is not free information, but I'll give you a hint.\"",
				3 => "Alican leans towards you, with its voice reduced to a whisper and a manic glint in its eye.",
				4 => "\"Mana's not the only thing out there. It's an engine of change, it can blow things up, it can reverse entropy, but it's not all there is. I'm studying *Starlight*. The inverse of mana... the energy of meaning, of memory, of connection. If you let me observe your efforts, I'll teach you how to use it.\"",
				5 => "\"This is Starsight. The ability to glimpse the weave of fate and meaning, grasping onto but a thin thread. If you encounter something beyond your own understanding, use it to borrow the knowledge you need. I have a feeling you'll be generating a lot of useful data for me...\"",
				6 => "\"I've got business in the Equatorial Ring, but before I go, one last word of advice... Something lurks beneath the nearby desert. The threads of memory converge in a great tangle - if you wish to understand Starlight, you must confront and decipher whatever's waiting for you there. I'll be watching.\"",
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

		private void SetFrame(int x, int y)
		{
			NPC.frame = new Rectangle(62 * x, 88 * y, 62, 88);
		}

		private Point16 GetFrame()
		{
			return new Point16(NPC.frame.X / 62, NPC.frame.Y / 88);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (InCutscene && CutsceneTimer < 140)
				DrawFlashingStar(spriteBatch, CutsceneTimer);

			var frame = new Rectangle(0, 88, 62, 88);
			SpriteEffects effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (visible)
				spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, NPC.Center + new Vector2(0, -10) - Main.screenPosition, NPC.frame, Lighting.GetColor((NPC.Center / 16).ToPoint()), 0, new Vector2(31, 44), 1, effects, 0);

			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (leaving)
			{
				DrawFlashingStar(spriteBatch, CutsceneTimer);
			}
		}

		public string GetHint()
		{
			return "What does he want with me?";
		}
	}
}