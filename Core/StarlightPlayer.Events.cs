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
		public delegate void ModifyHitByProjectileDelegate(Player player, Projectile proj, ref int damage, ref bool crit);
		public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
		public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
		{
			ModifyHitByProjectileEvent?.Invoke(Player, proj, ref damage, ref crit);
		}

		//for on-hit effects that require more specific effects, contact damage
		public delegate void ModifyHitByNPCDelegate(Player player, NPC NPC, ref int damage, ref bool crit);
		public static event ModifyHitByNPCDelegate ModifyHitByNPCEvent;
		public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
		{
			ModifyHitByNPCEvent?.Invoke(Player, NPC, ref damage, ref crit);
		}

		//for on-hit effects that run after modifyhitnpc and prehurt, contact damage
		public delegate void OnHitByNPCDelegate(Player player, NPC npc, int damage, bool crit);
		public static event OnHitByNPCDelegate OnHitByNPCEvent;
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			OnHitByNPCEvent?.Invoke(Player, npc, damage, crit);
		}

		//for on-hit effects that run after modifyhitnpc and prehurt, projectile damage
		public delegate void OnHitByProjectileDelegate(Player player, Projectile projectile, int damage, bool crit);
		public static event OnHitByProjectileDelegate OnHitByProjectileEvent;
		public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
		{
			OnHitByProjectileEvent?.Invoke(Player, projectile, damage, crit);
		}

		/// <summary>
		/// Use this event for the Player hitting an NPC with an Item directly (true melee).
		/// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
		/// </summary>
		public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
		public delegate void ModifyHitNPCDelegate(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit);
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Item, consider using ModifyHitNPC instead */
		{
			AddHitPacket(null, target, damage, knockback, crit);
			ModifyHitNPCEvent?.Invoke(Player, Item, target, ref damage, ref knockback, ref crit);
		}

		/// <summary>
		/// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
		/// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
		/// </summary>
		public static event ModifyHitNPCWithProjDelegate ModifyHitNPCWithProjEvent;
		public delegate void ModifyHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Projectile, consider using ModifyHitNPC instead */
		{
			AddHitPacket(proj, target, damage, knockback, crit);
			ModifyHitNPCWithProjEvent?.Invoke(Player, proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		/// <summary>
		/// Use this event for the Player hitting an NPC with an Item directly (true melee).
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiPlayer.
		/// </summary>
		public static event OnHitNPCDelegate OnHitNPCEvent;
		public delegate void OnHitNPCDelegate(Player player, Item Item, NPC target, int damage, float knockback, bool crit);
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Item, consider using OnHitNPC instead */
		{
			OnHitNPCEvent?.Invoke(Player, Item, target, damage, knockback, crit);
			SendHitPacket();
		}

		/// <summary>
		/// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
		/// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiPlayer.
		/// </summary>
		public static event OnHitNPCWithProjDelegate OnHitNPCWithProjEvent;
		public delegate void OnHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit);
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Projectile, consider using OnHitNPC instead */
		{
			OnHitNPCWithProjEvent?.Invoke(Player, proj, target, damage, knockback, crit);
			SendHitPacket();
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
		public delegate bool PreHurtDelegate(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter);
		/// <summary>
		/// If any PreHurtEvent returns false, the default behavior is overridden.
		/// </summary>
		public static event PreHurtDelegate PreHurtEvent;
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)/* tModPorter Override ImmuneTo, FreeDodge or ConsumableDodge instead to prevent taking damage */
		{
			if (PreHurtEvent != null)
			{
				bool result = true;
				foreach (PreHurtDelegate del in PreHurtEvent.GetInvocationList())
				{
					result &= del(Player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
				}

				return result;
			}

			return true;
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
			PreHurtEvent = null;
			PostUpdateRunSpeedsEvent = null;
			ResetEffectsEvent = null;
			ModifyDrawInfoEvent = null;

			spawners = null;
		}
	}
}