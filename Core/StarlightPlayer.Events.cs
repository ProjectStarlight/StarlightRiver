using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
	{
		private static Hook ModifyHitNPCWithProjHook;
		private static Hook ModifyPlayerHitNPCWithItemHook;

		//for Can-Use effects, runs before the item is used, return false to stop item use
		public delegate bool CanUseItemDelegate(Player player, Item item);
		public static event CanUseItemDelegate CanUseItemEvent;
		public override bool CanUseItem(Item item)
		{
			if (CanUseItemEvent != null)
			{
				bool result = true;
				foreach (CanUseItemDelegate del in CanUseItemEvent.GetInvocationList())
				{
					result &= del(Player, item);
				}

				return result;
			}

			return base.CanUseItem(item);
		}

		//for on-hit effects that require more specific effects, Projectiles
		public delegate void ModifyHitByProjectileDelegate(Player player, Projectile proj, ref Player.HurtModifiers modifiers);
		public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
		public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
		{
			ModifyHitByProjectileEvent?.Invoke(Player, proj, ref modifiers);
		}

		//for on-hit effects that require more specific effects, contact damage
		public delegate void ModifyHitByNPCDelegate(Player player, NPC NPC, ref Player.HurtModifiers modifiers);
		public static event ModifyHitByNPCDelegate ModifyHitByNPCEvent;
		public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
		{
			ModifyHitByNPCEvent?.Invoke(Player, npc, ref modifiers);
		}

		//for on-hit effects that run after modifyhitnpc and prehurt, contact damage
		public delegate void OnHitByNPCDelegate(Player player, NPC npc, Player.HurtInfo hurtInfo);
		public static event OnHitByNPCDelegate OnHitByNPCEvent;
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			OnHitByNPCEvent?.Invoke(Player, npc, hurtInfo);
		}

		//for on-hit effects that run after modifyhitnpc and prehurt, projectile damage
		public delegate void OnHitByProjectileDelegate(Player player, Projectile proj, Player.HurtInfo hurtInfo);
		public static event OnHitByProjectileDelegate OnHitByProjectileEvent;
		public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
		{
			OnHitByProjectileEvent?.Invoke(Player, proj, hurtInfo);
		}

		/// <summary>
		/// Use this event for the Player hitting an NPC with an Item directly (true melee).
		/// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
		/// call StarlightPlayer.SetHitPacketStatus to sync if this has an effect beyond editting ref variables.
		/// </summary>
		public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
		public delegate void ModifyHitNPCDelegate(Player player, Item Item, NPC target, ref NPC.HitModifiers modifiers);
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			ModifyHitNPCEvent?.Invoke(Player, item, target, ref modifiers);
		}

		/// <summary>
		/// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
		/// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
		/// call StarlightPlayer.SetHitPacketStatus to sync if this has an effect beyond editting ref variables.
		/// </summary>
		public static event ModifyHitNPCWithProjDelegate ModifyHitNPCWithProjEvent;
		public delegate void ModifyHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers);
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			ModifyHitNPCWithProjEvent?.Invoke(Player, proj, target, ref modifiers);
		}

		/// <summary>
		/// Use this event for the Player hitting an NPC with an Item directly (true melee).
		/// call StarlightPlayer.SetHitPacketStatus to sync if this has an effect for multiPlayer.
		/// </summary>
		public static event OnHitNPCDelegate OnHitNPCEvent;
		public delegate void OnHitNPCDelegate(Player player, Item Item, NPC target, NPC.HitInfo hit, int damageDone);
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			OnHitNPCEvent?.Invoke(Player, item, target, hit, damageDone);
			SendHitPacket(hit, damageDone);
		}

		/// <summary>
		/// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
		/// call StarlightPlayer.SetHitPacketStatus to sync if this has an effect for multiPlayer.
		/// </summary>
		public static event OnHitNPCWithProjDelegate OnHitNPCWithProjEvent;
		public delegate void OnHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone);
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			OnHitNPCWithProjEvent?.Invoke(Player, proj, target, hit, damageDone);
			SendHitPacket(hit, damageDone);
		}

		public delegate void NaturalLifeRegenDelegate(Player player, ref float regen);
		public static event NaturalLifeRegenDelegate NaturalLifeRegenEvent;
		public override void NaturalLifeRegen(ref float regen)
		{
			NaturalLifeRegenEvent?.Invoke(Player, ref regen);
		}

		public delegate void UpdateLifeRegenDelegate(Player player);
		public static event UpdateLifeRegenDelegate UpdateLifeRegenEvent;
		public override void UpdateLifeRegen()
		{
			UpdateLifeRegenEvent?.Invoke(Player);
		}

		public delegate void PostUpdateDelegate(Player player);
		public static event PostUpdateDelegate PostUpdateEvent;

		public delegate void PostDrawDelegate(Player player, SpriteBatch spriteBatch);
		public static event PostDrawDelegate PostDrawEvent;
		public void PostDraw(Player player, SpriteBatch spriteBatch)
		{
			PostDrawEvent?.Invoke(player, spriteBatch);
		}

		public delegate void PreDrawDelegate(Player player, SpriteBatch spriteBatch);
		public static event PreDrawDelegate PreDrawEvent;
		public void PreDraw(Player player, SpriteBatch spriteBatch)
		{
			PreDrawEvent?.Invoke(player, spriteBatch);
		}

		//this is the grossest one. I am sorry, little ones.
		public delegate void ModifyHurtDelegate(Player player, ref Player.HurtModifiers modifiers);
		/// <summary>
		/// If any PreHurtEvent returns false, the default behavior is overridden.
		/// </summary>
		public static event ModifyHurtDelegate ModifyHurtEvent;
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			ModifyHurtEvent?.Invoke(Player, ref modifiers);
		}

		public delegate void PostHurtDelegate(Player player, Player.HurtInfo info);
		public static event PostHurtDelegate PostHurtEvent;
		public override void PostHurt(Player.HurtInfo info)
		{
			PostHurtEvent?.Invoke(Player, info);
		}

		public delegate bool ImmuneToDelegate(Player player, PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable);
		public static event ImmuneToDelegate ImmuneToEvent;
		public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
		{
			bool result = false;

			if (ImmuneToEvent is null)
				return result;

			foreach (ImmuneToDelegate del in ImmuneToEvent.GetInvocationList())
			{
				result &= del.Invoke(Player, damageSource, cooldownCounter, dodgeable);
			}

			return result;
		}

		public delegate void PreUpdateBuffsDelegate(Player player);
		public static event PreUpdateBuffsDelegate PreUpdateBuffsEvent;
		public override void PreUpdateBuffs()
		{
			PreUpdateBuffsEvent?.Invoke(Player);
		}

		public delegate void PostUpdateRunSpeedsDelegate(Player player);
		public static event PostUpdateRunSpeedsDelegate PostUpdateRunSpeedsEvent;
		public override void PostUpdateRunSpeeds()
		{
			PostUpdateRunSpeedsEvent?.Invoke(Player);
		}

		public delegate void ModifyDrawInfoDelegate(ref PlayerDrawSet drawInfo);
		public static event ModifyDrawInfoDelegate ModifyDrawInfoEvent;
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			ModifyDrawInfoEvent?.Invoke(ref drawInfo);
		}

		public delegate void PreUpdateMovementDelegate(Player player);
		public static event PreUpdateMovementDelegate PreUpdateMovementEvent;
		public override void PreUpdateMovement()
		{
			PreUpdateMovementEvent?.Invoke(Player);
		}

		public delegate bool FreeDodgeDelegate(Player player, Player.HurtInfo info);
		public static event FreeDodgeDelegate FreeDodgeEvent;
		public override bool FreeDodge(Player.HurtInfo info)
		{
			bool result = false;

			if (FreeDodgeEvent is null)
				return result;

			foreach (FreeDodgeDelegate del in FreeDodgeEvent.GetInvocationList())
			{
				result |= del.Invoke(Player, info);
			}

			return result;
		}

		public delegate void Orig_OnModifyHitNPCWithProj(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers);
		public delegate void Orig_OnModifyPlayerHitNPCWithItem(Player player, Item sItem, NPC target, ref NPC.HitModifiers modifiers);

		/// <summary>
		/// Used to capture a proj hit before ANY processing occurs for generating a hitpacket.
		/// normal order is projectile -> NPC -> player with all modify overrides occuring first followed by onhits after refs are finalized 
		/// </summary>
		public static void OnModifyHitNPCWithProj(Orig_OnModifyHitNPCWithProj orig, Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				Main.player[projectile.owner].TryGetModPlayer(out StarlightPlayer starlightPlayer);
				starlightPlayer.AddHitPacket(projectile, target);
			}

			orig(projectile, target, ref modifiers);
		}

		/// <summary>
		/// Used to capture an item hit before ANY processing occurs for generating a hitpacket.
		/// normal order is projectile -> NPC -> player with all modify overrides occuring first followed by onhits after refs are finalized 
		/// </summary>
		public static void OnModifyPlayerHitNPCWithItem(Orig_OnModifyPlayerHitNPCWithItem orig, Player player, Item sItem, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				player.TryGetModPlayer(out StarlightPlayer starlightPlayer);
				starlightPlayer.AddHitPacket(null, target);
			}

			orig(player, sItem, target, ref modifiers);
		}

		/// <summary>
		/// This load creates a hook into the beginning of a hit processing so we can snapshot the initial state 
		/// to send a packet to other clients and server to have them process all the custom onhit logic too where needed
		/// </summary>
		public override void Load()
		{
			//Order of operations for processing a projectile hit is as follows:
			// 1. ProjectileLoader.ModifyHitNPC (event in StarlightProjectile.Events)
			// 2. NPCLoader.ModifyHitByProjectile (event in StarlightNPC.Events) 
			// 3. PlayerLoader.ModifyHitNPCWithProj (event in this class) <-- only if projectile has a player owner, will be skipped for server projs
			// 4. ProjectileLoader.OnHitNPC (event in StarlightProjectile.Events)
			// 5. NPCLoader.OnHitByProjectile (event in StarlightNPC.Events)
			// 6. PlayerLoader.OnHitNPCWithProj (Event in this class) <-- only if projectile has a player owner, will be skipped for server projs

			//order of operations for processing an item hit (true melee) is as follows
			// 1. ItemLoader.ModifyHitNPC (event in StarlightItem.Events)
			// 2. NPCLoader.ModifyHitByItem (event in StarlightNPC.Events)
			// 3. PlayerLoader.ModifyHitNPCWithItem (event in this class)
			// 4. ItemLoader.OnHitNPC (event in StarlightItem.Events)
			// 5. NPCLoader.OnHitByItem (event in StarlightNPC.Events)
			// 6. PlayerLoader.OnHitNPCWithItem (event in this class)

			//No guarantees on ordering within each step unless we write something for that (probably not needed, just use the right event?)

			MethodInfo ModifyHitNPCWithProjMethod = typeof(CombinedHooks).GetMethod("ModifyHitNPCWithProj", BindingFlags.Public | BindingFlags.Static);
			ModifyHitNPCWithProjHook = new Hook(ModifyHitNPCWithProjMethod, OnModifyHitNPCWithProj);

			MethodInfo ModifyPlayerHitNPCWithItemMethod = typeof(CombinedHooks).GetMethod("ModifyPlayerHitNPCWithItem", BindingFlags.Public | BindingFlags.Static);
			ModifyPlayerHitNPCWithItemHook = new Hook(ModifyPlayerHitNPCWithItemMethod, OnModifyPlayerHitNPCWithItem);
		}

		public override void Unload()
		{
			CanUseItemEvent = null;
			ModifyHitByNPCEvent = null;
			ModifyHitByProjectileEvent = null;
			ModifyHitNPCEvent = null;
			ModifyHitNPCWithProjEvent = null;
			NaturalLifeRegenEvent = null;
			OnHitByNPCEvent = null;
			OnHitByProjectileEvent = null;
			OnHitNPCEvent = null;
			OnHitNPCWithProjEvent = null;
			PostDrawEvent = null;
			PostUpdateEquipsEvent = null;
			PostUpdateEvent = null;
			PreDrawEvent = null;
			ModifyHurtEvent = null;
			PostUpdateRunSpeedsEvent = null;
			ResetEffectsEvent = null;
			ModifyDrawInfoEvent = null;
			PreUpdateMovementEvent = null;

			ModifyHitNPCWithProjHook.Dispose();
			ModifyHitNPCWithProjHook = null;

			ModifyPlayerHitNPCWithItemHook.Dispose();
			ModifyPlayerHitNPCWithItemHook = null;

			spawners = null;
		}
	}
}