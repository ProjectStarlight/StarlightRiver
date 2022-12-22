using StarlightRiver.Core.Systems.CameraSystem;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
	{
		private void SpawnAnimation()
		{
			NPC.noGravity = true;

			if (AttackTimer == 0)
				CameraSystem.shake += 6;

			if (AttackTimer < 60)
			{
				NPC.position.Y += (AttackTimer - 45) * 0.3f;
				NPC.scale = 1 - AttackTimer / 60f * 0.25f;
			}

			if (AttackTimer == 60)
				CameraSystem.shake += 12;
		}

		private void JumpBackAnimation()
		{

			if (AttackTimer > 410)
			{
				Phase = (int)Phases.DirectPhase;
				ResetAttack();
				NPC.dontTakeDamage = false;
				NPC.noGravity = false;
				AttackPhase = -1;
			}
		}
	}
}