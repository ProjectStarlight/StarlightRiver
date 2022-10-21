using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public partial class StarlightProjectile : GlobalProjectile
	{
		public delegate void PostAIDelegate(Projectile Projectile);
		public static event PostAIDelegate PostAIEvent;
		public override void PostAI(Projectile Projectile)
		{
			PostAIEvent?.Invoke(Projectile);
		}

		public delegate void KillDelegate(Projectile Projectile, int timeLeft);
		public static event KillDelegate KillEvent;
		public override void Kill(Projectile Projectile, int timeLeft)
		{
			KillEvent?.Invoke(Projectile, timeLeft);
		}

		public override void Unload()
		{
			KillEvent = null;
			ModifyHitNPCEvent = null;
		}
	}
}
