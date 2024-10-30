using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class DeadBrain
	{
		public void Intro()
		{
			// Magnetize the players into the fight here
			foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, thinker.Center) < 1500))
			{
				player.position += (thinker.Center - player.Center) * 0.1f * (Vector2.Distance(player.Center, thinker.Center) / 1500f);
			}

			// Lighting at chain end
			if (Timer <= 480)
				Lighting.AddLight(chainTarget, new Vector3(0.8f, 0.5f, 0.2f));

			if (Timer == 1)
			{
				NPC.Center = thinker.Center + Vector2.UnitY.RotatedBy(-0.5f) * ThisThinker.hurtRadius;
				savedPos = NPC.Center;
				chainTarget = thinker.Center;

				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(90, thinker.Center);
					ZoomHandler.SetZoomAnimation(1.5f, 90);
				}

				extraChunkRadius = 2;
				staggeredExtraChunkRadius = 1;
			}

			if (Timer == 100)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(30, NPC.Center);
					ZoomHandler.SetZoomAnimation(1.2f, 30);
				}
			}

			if (Timer > 100 && Timer < 120)
			{
				chainTarget = Vector2.Lerp(thinker.Center, NPC.Center + Vector2.UnitY * 90, (Timer - 100) / 20f);
			}

			if (Timer == 120)
			{
				Helpers.Helper.PlayPitched("Impacts/StabFleshy", 1f, -0.8f, NPC.Center);
			}

			// Chain needs to be linked after this point
			if (Timer >= 120)
				chainTarget = NPC.Center + Vector2.UnitY * 90;

			// Move brain into position
			if (Timer > 160 && Timer < 360)
			{
				var prog = (Timer - 160) / 200f;
				prog = Helpers.Helper.BezierEase(prog);
				NPC.Center = Vector2.Lerp(savedPos, thinker.Center + new Vector2(200, -200), Helpers.Helper.SwoopEase(prog));
			}

			if (Timer > 160 && Timer <= 240)
			{
				var prog = 1f - (Timer - 160) / 80f;
				extraChunkRadius = prog * 2;
				staggeredExtraChunkRadius = prog;
			}

			// Move camera to follow
			if (Timer == 160)
			{
				if (IsInArena(Main.LocalPlayer))
				{
					CameraSystem.MoveCameraOut(200, thinker.Center, (a, b, c) => Vector2.Lerp(a, b, Helpers.Helper.SwoopEase(c)));
					ZoomHandler.SetZoomAnimation(Main.GameZoomTarget, 200);
				}
			}

			if (Timer > 430 && Timer < 480)
			{
				shieldOpacity = Helpers.Helper.SwoopEase((Timer - 430) / 50f) * 0.4f;
			}

			// Neurysms
			if (Timer > 280 && Timer <= 440)
			{		
				for (int k = 0; k < neurisms.Count; k++)
				{
					float lerp = k / (neurisms.Count - 1f);
					float rot = 6.28f * lerp;

					if (Timer == 280 + k * 5)
					{
						neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * 200;
						(neurisms[k].ModNPC as Neurysm).State = 2;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}

					if (Timer >= 360 && Timer <= 420)
					{
						float prog = Helpers.Helper.BezierEase((Timer - 360) / 60f);
						var center = Vector2.Lerp(thinker.Center, NPC.Center, prog);
						neurisms[k].Center = center + Vector2.UnitX.RotatedBy(rot + prog * 1.57f) * (200 - (70f * prog));
						(neurisms[k].ModNPC as Neurysm).State = 0;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}

					if (Timer == 420)
					{
						neurisms[k].velocity *= 0;
						(neurisms[k].ModNPC as Neurysm).State = 1;
						(neurisms[k].ModNPC as Neurysm).Timer = 0;
					}
				}
			}

			if (Timer == 270)
			{
				if (Main.netMode != NetmodeID.Server)
					UILoader.GetUIState<TextCard>().Display(thinker.FullName, Main.rand.NextBool(10000) ? "Thinking of you <3" : "& the Dead Brain", null, 270, 1.25f); //intro text
			}

			if (Timer == 200)
			{
				(thinker.ModNPC as TheThinker)?.CreateArena();
			}

			if (Timer >= 220)
			{
				if (arenaFade < 120)
					arenaFade++;
			}

			if (Timer == 500)
				CameraSystem.ReturnCamera(40);

			if (Timer == 540)
			{
				State = 2;
				Timer = 0;
				AttackTimer = 0;
			}
		}

		public void FirstPhaseTransition()
		{
			if (Timer == 1)
			{
				savedPos = NPC.Center;
				CameraSystem.DoPanAnimation(120, NPC.Center);
			}
		}
	}
}
