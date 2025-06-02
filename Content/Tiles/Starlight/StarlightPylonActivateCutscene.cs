using Microsoft.CodeAnalysis;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Starlight
{
	internal class StarlightPylonActivateCutsceneTrigger : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.boss;
		}

		public override void OnKill(NPC npc)
		{
			if (!ObservatorySystem.observatoryOpen)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					player.ActivateCutscene<StarlightPylonActivateCutscene>();
				}

				ObservatorySystem.observatoryOpen = true;
			}
		}
	}

	internal class StarlightPylonActivateCutscene : Cutscene
	{
		public override void InCutscene(Player player)
		{
			Vector2 center = ObservatorySystem.SideStructureWorld.Center();

			// Opening fade
			if (timer <= 60)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = timer / 60f;
			}

			if (timer == 60)
			{
				CameraSystem.TeleportCamera(center);
				ZoomHandler.SetZoomAnimation(1.5f, 70);
			}

			if (timer > 70 && timer <= 130)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = 1f - (timer - 70) / 60f;
			}

			if (timer == 240)
			{
				ObservatorySystem.pylonAppearsOn = true;

				Vector2 pylon = Main.PylonSystem.Pylons.FirstOrDefault(n => n.ModPylon is StarlightPylon).PositionInTiles.ToVector2() * 16;
				pylon += new Vector2(24, 24);

				SoundHelper.PlayPitched("Magic/HolyCastShort", 1f, 0.5f, pylon);
				SoundHelper.PlayPitched("Magic/Shadow1", 1f, 0.0f, pylon);
				SoundHelper.PlayPitched("Magic/Shadow2", 1f, 0.25f, pylon);

				for (int k = 0; k < 40; k++)
				{
					Vector2 off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(pylon + off * 16, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), off * Main.rand.NextFloat(30), 0, new Color(Main.rand.Next(30), 100 + Main.rand.Next(155), 255, 0), Main.rand.NextFloat(0.2f, 0.3f));
				}

				for (int k = 0; k < 4; k++)
				{
					Vector2 off = Vector2.UnitX.RotatedBy(k / 4f * 6.28f);
					Dust.NewDustPerfect(pylon + off * 16, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), off * 45, 0, new Color(Main.rand.Next(100), 100 + Main.rand.Next(155), 255, 0), Main.rand.NextFloat(0.2f, 0.3f));
				}
			}

			// Ending fade
			if (timer > 330)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = (timer - 330) / 60f;
			}

			if (timer == 390)
			{
				CameraSystem.TeleportCameraBack();
				ZoomHandler.ReturnZoom(1);
			}

			if (timer > 390)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = 1f - (timer - 390) / 60f;
			}

			if (timer >= 450)
			{
				player.DeactivateCutscene();
			}
		}
	}
}