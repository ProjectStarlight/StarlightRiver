namespace StarlightRiver.Core
{
	public partial class StarlightNPC : GlobalNPC
	{
		public delegate void PostAIDelegate(NPC NPC);
		public static event PostAIDelegate PostAIEvent;
		public override void PostAI(NPC NPC)
		{
			PostAIEvent?.Invoke(NPC);
		}

		public delegate void OnKillDelegate(NPC NPC);
		public static event OnKillDelegate OnKillEvent;
		public override void OnKill(NPC NPC)
		{
			OnKillEvent?.Invoke(NPC);
		}

		//these modify hit bys should only be used for editting the ref variables if you want them changed in a way that happens BEFORE Player on hit effects run. no extra effects will be synced in multiPlayer outside of the ref variables
		public delegate void ModifyHitByItemDelegate(NPC NPC, Player Player, Item Item, ref NPC.HitModifiers modifiers);
		public static event ModifyHitByItemDelegate ModifyHitByItemEvent;
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
		{
			ModifyHitByItemEvent?.Invoke(npc, player, item, ref modifiers);
		}

		public delegate void ModifyHitByProjectileDelegate(NPC NPC, Projectile Projectile, ref NPC.HitModifiers modifiers);
		public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			ModifyHitByProjectileEvent?.Invoke(npc, projectile, ref modifiers);
		}

		public delegate void ModifyHitPlayerDelegate(NPC NPC, Player target, ref Player.HurtModifiers modifiers);
		public static event ModifyHitPlayerDelegate ModifyHitPlayerEvent;
		public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
		{
			ModifyHitPlayerEvent?.Invoke(npc, target, ref modifiers);
		}

		public delegate void ResetEffectsDelegate(NPC NPC);
		public static event ResetEffectsDelegate ResetEffectsEvent;
		public override void ResetEffects(NPC NPC)
		{
			ResetEffectsEvent?.Invoke(NPC);
		}

		public delegate void ModifyNPCLootDelegate(NPC npc, NPCLoot npcloot);
		public static event ModifyNPCLootDelegate ModifyNPCLootEvent;
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			ModifyNPCLootEvent?.Invoke(npc, npcLoot);
		}

		public delegate void ModifyGlobalLootDelegate(GlobalLoot globalLoot);
		public static event ModifyGlobalLootDelegate ModifyGlobalLootEvent;
		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			ModifyGlobalLootEvent?.Invoke(globalLoot);
		}

		public delegate void UpdateLifeRegenDelegate(NPC npc, ref int damage);
		public static event UpdateLifeRegenDelegate UpdateLifeRegenEvent;
		public override void UpdateLifeRegen(NPC NPC, ref int damage)
		{
			UpdateLifeRegenEvent.Invoke(NPC, ref damage);
		}

		public override void Unload()
		{
			PostAIEvent = null;
			OnKillEvent = null;
			ModifyHitByItemEvent = null;
			ModifyHitByProjectileEvent = null;
			ModifyHitPlayerEvent = null;
			ResetEffectsEvent = null;
			ModifyNPCLootEvent = null;
			ModifyGlobalLootEvent = null;
			UpdateLifeRegenEvent = null;
		}
	}
}