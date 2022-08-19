using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

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
        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            ModifyHitByProjectileEvent?.Invoke(Player, proj, ref damage, ref crit);
        }

        //for on-hit effects that require more specific effects, contact damage
        public delegate void ModifyHitByNPCDelegate(Player player, NPC NPC, ref int damage, ref bool crit);
        public static event ModifyHitByNPCDelegate ModifyHitByNPCEvent;
        public override void ModifyHitByNPC(NPC NPC, ref int damage, ref bool crit)
        {
            ModifyHitByNPCEvent?.Invoke(Player, NPC, ref damage, ref crit);
        }

        //for on-hit effects that run after modifyhitnpc and prehurt, contact damage
        public delegate void OnHitByNPCDelegate(Player player, NPC npc, int damage, bool crit);
        public static event OnHitByNPCDelegate OnHitByNPCEvent;
        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            OnHitByNPCEvent?.Invoke(Player, npc, damage, crit);
        }

        //for on-hit effects that run after modifyhitnpc and prehurt, projectile damage
        public delegate void OnHitByProjectileDelegate(Player player, Projectile projectile, int damage, bool crit);
        public static event OnHitByProjectileDelegate OnHitByProjectileEvent;
        public override void OnHitByProjectile(Projectile projectile, int damage, bool crit)
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
        public override void ModifyHitNPC(Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            addHitPacket(null, target, damage, knockback, crit);
            ModifyHitNPCEvent?.Invoke(Player, Item, target, ref damage, ref knockback, ref crit);
        }


        /// <summary>
        /// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
        /// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
        /// </summary>
        public static event ModifyHitNPCWithProjDelegate ModifyHitNPCWithProjEvent;
        public delegate void ModifyHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            addHitPacket(proj, target, damage, knockback, crit);
            ModifyHitNPCWithProjEvent?.Invoke(Player, proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        /// <summary>
        /// Use this event for the Player hitting an NPC with an Item directly (true melee).
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiPlayer.
        /// </summary>
        public static event OnHitNPCDelegate OnHitNPCEvent;
        public delegate void OnHitNPCDelegate(Player player, Item Item, NPC target, int damage, float knockback, bool crit);
        public override void OnHitNPC(Item Item, NPC target, int damage, float knockback, bool crit)
        {
            OnHitNPCEvent?.Invoke(Player, Item, target, damage, knockback, crit);
            sendHitPacket();
        }


        /// <summary>
        /// Use this event for Projectile hitting NPCs for situations where a Projectile should be owned by a Player.
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiPlayer.
        /// </summary>
        public static event OnHitNPCWithProjDelegate OnHitNPCWithProjEvent;
        public delegate void OnHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit);
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            OnHitNPCWithProjEvent?.Invoke(Player, proj, target, damage, knockback, crit);
            sendHitPacket();
        }

        public delegate void NaturalLifeRegenDelegate(Player player, ref float regen);
        public static event NaturalLifeRegenDelegate NaturalLifeRegenEvent;
        public override void NaturalLifeRegen(ref float regen)
        {
            NaturalLifeRegenEvent?.Invoke(Player, ref regen);
        }

        public delegate void PostUpdateDelegate(Player player);
        public static event PostUpdateDelegate PostUpdateEvent;

        public delegate void PostDrawDelegate(Player player, SpriteBatch spriteBatch);
        public static event PostDrawDelegate PostDrawEvent;
        public void PostDraw(Player player, SpriteBatch spriteBatch)
        {
            PostDrawEvent?.Invoke(Player, Main.spriteBatch);
        }

        public delegate void PreDrawDelegate(Player player, SpriteBatch spriteBatch);
        public static event PreDrawDelegate PreDrawEvent;
        public void PreDraw(Player player, SpriteBatch spriteBatch)
        {
            PreDrawEvent?.Invoke(Player, Main.spriteBatch);
        }

        //this is the grossest one. I am sorry, little ones.
        public delegate bool PreHurtDelegate(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);
        /// <summary>
        /// If any PreHurtEvent returns false, the default behavior is overridden.
        /// </summary>
        public static event PreHurtDelegate PreHurtEvent;
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (PreHurtEvent != null)
            {
                bool result = true;
                foreach (PreHurtDelegate del in PreHurtEvent.GetInvocationList())
                {
                    result &= del(Player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
                }
                return result;
            }
            return true;
        }

        public delegate void PostUpdateRunSpeedsDelegate(Player player);
        public static event PostUpdateRunSpeedsDelegate PostUpdateRunSpeedsEvent;
        public override void PostUpdateRunSpeeds()
        {
            PostUpdateRunSpeedsEvent?.Invoke(Player);
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

            spawners = null;
		}
	}
}
