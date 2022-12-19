using StarlightRiver.Core.Systems.CameraSystem;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
	{
		private void JumpBackAnimation()
		{
			AttackTimer++;

			if (AttackTimer == 40)
				CameraSystem.shake = 8;

			if (AttackTimer > 38 && AttackTimer < 160)
				CameraSystem.shake += 2;

			if (AttackTimer > 410)
			{
				Phase = (int)Phases.DirectPhase;
				ResetAttack();
				NPC.dontTakeDamage = false;
				AttackPhase = -1;
			}
		}
	}
}