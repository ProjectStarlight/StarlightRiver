using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Starlight
{
	class Alican : ModNPC
	{
		public bool visible = true;

		public Player talkingTo;
		public DialogManager manager;

		public override string Texture => "StarlightRiver/Assets/NPCs/Starlight/Alican";

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];
		public ref float CutsceneTimer => ref NPC.ai[2];

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
			NPC.noGravity = false;
			NPC.netAlways = true;

			NPC.frame = new Rectangle(0, 0, 40, 64);

			visible = true;

			manager = new("AlicanDialog.json", NPC);
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void AI()
		{
			Timer++;

			// This guarantees all clients get relevant info about this NPC eventually
			if (Main.netMode == NetmodeID.Server && Main.GameUpdateCount % 60 == 0)
				NPC.netUpdate = true;

			if (talkingTo != null && talkingTo.TalkNPC != NPC)
			{
				talkingTo = null;
				DialogUI.CloseDialogue();
			}

			if (visible)
			{
				if (Timer % 50 < 25)
					SetFrame(0, 11);
				else
					SetFrame(0, 12);

				NPC.TargetClosest();
				NPC.direction = Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1;

				/*if (Main.rand.NextBool(6))
					Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-16, 16), 32), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.25f), 0, new Color(100, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.35f, 0.45f));

				if (Timer % 120 == 0)
					Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.Next(-32, 32), 24), ModContent.DustType<Dusts.VerticalGlow>(), Vector2.Zero, 0, new Color(20, Main.rand.Next(150, 255), 255), Main.rand.NextFloat(0.9f, 1.1f));
				*/

				Lighting.AddLight(NPC.Center, new Vector3(0.1f, 0.15f, 0.25f) * 2);
			}
		}

		public override bool CanChat()
		{
			return Main.LocalPlayer.InModBiome<ObservatoryBiome>();
		}

		public override string GetChat()
		{
			talkingTo = Main.LocalPlayer;

			if (State == 0)
				manager.Start("Intro1");

			if (State == 1)
				manager.Start("AfterTutorial1");

			if (State == 2)
				manager.Start("AfterTutorialRepeating");

			return "";
		}

		/// <summary>
		/// Invoked by dialogue manager to give the player starlight
		/// </summary>
		public void UnlockStarsight()
		{
			Main.LocalPlayer.GetHandler().Unlock<HintAbility>();
			Stamina.gainAnimationTimer = 240;
		}

		/// <summary>
		/// Invoked by dialogue manager to give the player starsight and display the tutorial
		/// </summary>
		public void StarsightTutorial()
		{
			Main.LocalPlayer.DeactivateCutscene();

			AlicanSafetySystem.IntendedAlicanPhase = 1;
			State = 1;

			if(StarlightRiver.Instance.AbilityKeys.Get<HintAbility>().GetAssignedKeys().Count <= 0)
				KeybindHelper.OpenKeybindsWithHelp();

			TutorialManager.ActivateTutorial("Starsight");
		}

		/// <summary>
		/// Invoked by dialogue manager to set state
		/// </summary>
		public void FinishTutorial()
		{
			AlicanSafetySystem.IntendedAlicanPhase = 2;
			State = 2;
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
			var frame = new Rectangle(0, 88, 62, 88);
			SpriteEffects effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (visible)
				spriteBatch.Draw(Assets.NPCs.Starlight.Alican.Value, NPC.Center + new Vector2(0, -10) - Main.screenPosition, NPC.frame, Lighting.GetColor((NPC.Center / 16).ToPoint()), 0, new Vector2(31, 44), 1, effects, 0);

			if ((
				State == 0 ||
				State == 1)
				&& talkingTo is null)
			{
				Texture2D exclaim = Assets.Misc.Exclaim.Value;
				Vector2 exclaimPos = NPC.Center + Vector2.UnitY * -66 - Main.screenPosition;
				exclaimPos.Y += (float)Math.Sin(Main.GameUpdateCount * 0.025f) * 5;
				spriteBatch.Draw(exclaim, exclaimPos, null, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f, exclaim.Size() / 2f, 1, 0, 0);

				float pulseTime = Main.GameUpdateCount % 60 < 50 ? 0 : (Main.GameUpdateCount % 60 - 50) / 10f;

				spriteBatch.Draw(exclaim, exclaimPos, null, Color.White * (1 - pulseTime), (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f, exclaim.Size() / 2f, 1 + pulseTime, 0, 0);
			}

			return false;
		}
	}
}