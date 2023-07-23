using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using System.Linq;

namespace StarlightRiver.Content.NPCs.Starlight
{
	internal class CrowCutsceneOne : Cutscene
	{
		public override void InCutscene(Player player)
		{
			NPC alican = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<Crow>());

			// Abort if we cant find bird
			if (alican is null)
				player.DeactivateCutscene();

			player.GetHandler().Stamina = 0;
			player.GetHandler().SetStaminaRegenCD(0);

			player.immuneNoBlink = true;

			if (timer == 1)
				CameraSystem.MoveCameraOut(30, alican.Center + Vector2.UnitY * 120, Vector2.SmoothStep);
		}

		public override void EndCutscene(Player player)
		{
			CameraSystem.ReturnCamera(30, Vector2.SmoothStep);
		}
	}
}
