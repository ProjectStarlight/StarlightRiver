namespace StarlightRiver.Core
{
	public partial class StarlightProjectile : GlobalProjectile
	{
		public delegate void ModifyHitNPCDelegate(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
		public override void ModifyHitNPC(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			ModifyHitNPCEvent?.Invoke(Projectile, target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

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
			ModifyHitNPCEvent = null;
			KillEvent = null;
			ModifyHitNPCEvent = null;
		}
	}
}
