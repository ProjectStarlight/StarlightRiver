namespace StarlightRiver.Core
{
	public partial class StarlightProjectile : GlobalProjectile
	{
		public delegate void ModifyHitNPCDelegate(Projectile Projectile, NPC target, ref NPC.HitModifiers modifiers);
		public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			ModifyHitNPCEvent?.Invoke(projectile, target, ref modifiers);
		}

		public delegate void OnHitNPCDelegate(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone);
		public static event OnHitNPCDelegate OnHitNPCEvent;
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			OnHitNPCEvent?.Invoke(projectile, target, hit, damageDone);
		}

		public delegate void OnHitPlayerDelegate(Projectile projectile, Player target, Player.HurtInfo info);
		public static event OnHitPlayerDelegate OnHitPlayerEvent;
		public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
		{
			OnHitPlayerEvent?.Invoke(projectile, target, info);
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

		public delegate void PostDrawDelegate(Projectile projectile, Color lightColor);
		public static event PostDrawDelegate PostDrawEvent;
		public override void PostDraw(Projectile projectile, Color lightColor)
		{
			PostDrawEvent?.Invoke(projectile, lightColor);
		}

		public override void Unload()
		{
			ModifyHitNPCEvent = null;
			OnHitNPCEvent = null;
			OnHitPlayerEvent = null;
			PostAIEvent = null;
			KillEvent = null;
			PostDrawEvent = null;
		}
	}
}