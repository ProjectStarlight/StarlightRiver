using Microsoft.CodeAnalysis;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Starlight
{
	internal class StarlightPylonActivateCutscene : Cutscene
	{
		public override void InCutscene(Player player)
		{
			Vector2 center = ModContent.GetInstance<ObservatorySystem>().ObservatoryRoomWorld.Center();

			// Opening fade
			if (timer < 30)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = timer / 30f;
			}

			if (timer == 30)
			{
				CameraSystem.TeleportCamera(center);
			}

			if (timer > 30 && timer < 90)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = 1f - (timer - 30) / 60f;
			}



			// Ending fade
			if (timer > 240)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = (timer - 240) / 60f;
			}

			if (timer == 300)
			{
				StarlightWorld.Flag(WorldFlags.ThinkerBossOpen);
				CameraSystem.TeleportCameraBack();
			}

			if (timer > 300)
			{
				Fadeout.color = Color.Black;
				Fadeout.opacity = 1f - (timer - 300) / 60f;
			}

			if (timer >= 360)
			{
				player.DeactivateCutscene();
			}
		}
	}
}
