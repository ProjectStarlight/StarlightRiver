using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public partial class StarlightNPC : GlobalNPC
	{
		public override void Load()
		{
			MethodInfo DrawHealthBarMethod = typeof(NPCLoader).GetMethod("DrawHealthBar", BindingFlags.Public | BindingFlags.Static);
			MonoModHooks.Modify(DrawHealthBarMethod, PostDrawHealthBarHook);
		}

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

		public delegate void OnHitByItemDelegate(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone);
		public static event OnHitByItemDelegate OnHitByItemEvent;
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			OnHitByItemEvent?.Invoke(npc, player, item, hit, damageDone);
		}

		public delegate void OnHitByProjectileDelegate(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone);
		public static event OnHitByProjectileDelegate OnHitByProjectileEvent;
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			OnHitByProjectileEvent?.Invoke(npc, projectile, hit, damageDone);
		}

		public delegate void ModifyIncomingHitDelegate(NPC npc, ref NPC.HitModifiers modifiers);
		public static event ModifyIncomingHitDelegate ModifyIncomingHitEvent;
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			ModifyIncomingHitEvent?.Invoke(npc, ref modifiers);
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

		private void PostDrawHealthBarHook(ILContext il)
		{
			ILCursor c = new(il);
			c.TryGotoNext(MoveType.AfterLabel, n => n.MatchLdcI4(1), n => n.MatchRet()); // Go to end
			c.Index++;
			c.Emit(OpCodes.Pop);

			// Draw default health bar first
			c.Emit(OpCodes.Ldarg, 0); // NPC param
			c.Emit(OpCodes.Ldarg, 1); // scale param
			c.Emit(OpCodes.Ldind_R4);
			c.Emit(OpCodes.Ldloc, 0); // position which is stored as a local for some annoying reason			
			c.EmitDelegate(DrawDefaultBar);

			c.Emit(OpCodes.Ldarg, 0); // NPC param
			c.Emit(OpCodes.Ldarg, 1); // scale param
			c.Emit(OpCodes.Ldind_R4);
			c.Emit(OpCodes.Ldloc, 0); // position which is stored as a local for some annoying reason
			c.EmitDelegate(PostDrawHealthBar); // emit the delegate calling our event
			c.Emit(OpCodes.Ldc_I4_0);
		}

		public static void DrawDefaultBar(NPC NPC, float scale, Vector2 position)
		{
			if (NPC.realLife != -1 && NPC.realLife != NPC.whoAmI)
				return;

			float bright = Lighting.Brightness((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
			Main.instance.DrawHealthBar((int)position.X, (int)position.Y, NPC.life, NPC.lifeMax, bright, scale);
		}

		public delegate void PostDrawHealthBarDelegate(NPC NPC, byte hbPosition, float scale, Vector2 position);
		public static event PostDrawHealthBarDelegate PostDrawHealthBarEvent;
		public static void PostDrawHealthBar(NPC NPC, float scale, Vector2 position)
		{
			if (NPC.realLife != -1 && NPC.realLife != NPC.whoAmI)
				return;

			PostDrawHealthBarEvent?.Invoke(NPC, Main.HealthBarDrawSettings, scale, position);
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