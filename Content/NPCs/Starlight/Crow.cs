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
		public override string Texture => AssetDirectory.Debug;

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
			NPC.width = 64;
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
		}

		public override void AI()
		{
			Timer++;

			if (InCutscene)
				CutsceneTimer++;

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

				}
			}
		}

		private void FirstEncounter()
		{
			Main.LocalPlayer.GetHandler().Stamina = 0;
			Main.LocalPlayer.GetHandler().SetStaminaRegenCD(0);

			if (CutsceneTimer == 1)
				CameraSystem.MoveCameraOut(30, NPC.Center, Vector2.SmoothStep);

			if (CutsceneTimer == 60) // First encounter
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
								StarlightEventSequenceSystem.sequence = 1;
								StarlightEventSequenceSystem.willOccur = false;

								Main.LocalPlayer.GetHandler().GetAbility(out HintAbility hint);
								UILoader.GetUIState<TextCard>().Display("[PH] Hint", "[PH] Press key to investigate the world", hint);
							});
						});
					}
				});
			}
		}

		private string GetIntroDialogue()
		{
			return TextState switch
			{
				0 => "Greetings wanderer. I have been waiting for a very long time to meet you.",
				1 => "Placeholder Text 2",
				2 => "Placeholder Text 3",
				3 => "Placeholder Text 4",
				_ => "This text should never be seen! Please report to https://github.com/ProjectStarlight/StarlightRiver/issues",
			};
		}
	}
}