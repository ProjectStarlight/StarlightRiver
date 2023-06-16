using Terraria.DataStructures;

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
	{
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
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
		/// </summary>
		public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
		public delegate void ModifyHitNPCDelegate(Player player, Item Item, NPC target, ref NPC.HitModifiers modifiers);
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			AddHitPacket(null, target, modifiers);
			ModifyHitNPCEvent?.Invoke(Player, item, target, ref modifiers);
		}

		/// <summary>
		/// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
		/// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
		/// </summary>
		public static event ModifyHitNPCWithProjDelegate ModifyHitNPCWithProjEvent;
		public delegate void ModifyHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers);
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			ModifyHitNPCWithProjEvent?.Invoke(Player, proj, target, ref modifiers);
		}

		/// <summary>
		/// Use this event for the Player hitting an NPC with an Item directly (true melee).
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiPlayer.
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
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiPlayer.
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

			spawners = null;
		}
	}
}