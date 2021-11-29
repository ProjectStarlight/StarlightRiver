using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
    {
        //for on-hit effects that require more specific effects, projectiles
        public delegate void ModifyHitByProjectileDelegate(Player player, Projectile proj, ref int damage, ref bool crit);
        public static event ModifyHitByProjectileDelegate ModifyHitByProjectileEvent;
        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
        {
            ModifyHitByProjectileEvent?.Invoke(player, proj, ref damage, ref crit);
        }

        //for on-hit effects that require more specific effects, contact damage
        public delegate void ModifyHitByNPCDelegate(Player player, NPC npc, ref int damage, ref bool crit);
        public static event ModifyHitByNPCDelegate ModifyHitByNPCEvent;
        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
            ModifyHitByNPCEvent?.Invoke(player, npc, ref damage, ref crit);
        }


        /// <summary>
        /// Use this event for the player hitting an NPC with an item directly (true melee).
        /// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
        /// </summary>
        public static event ModifyHitNPCDelegate ModifyHitNPCEvent;
        public delegate void ModifyHitNPCDelegate(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit);
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            addHitPacket(null, target, damage, knockback, crit);
            ModifyHitNPCEvent?.Invoke(player, item, target, ref damage, ref knockback, ref crit);
        }


        /// <summary>
        /// Use this event for projectile hitting npcs for situations where a projectile should be owned by a player.
        /// This happens before the onHit hook and should be used if the effect modifies the any of the ref variables otherwise stick to the onHit.
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect beyond editting ref variables.
        /// </summary>
        public static event ModifyHitNPCWithProjDelegate ModifyHitNPCWithProjEvent;
        public delegate void ModifyHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            addHitPacket(proj, target, damage, knockback, crit);
            ModifyHitNPCWithProjEvent?.Invoke(player, proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        /// <summary>
        /// Use this event for the player hitting an NPC with an item directly (true melee).
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiplayer.
        /// </summary>
        public static event OnHitNPCDelegate OnHitNPCEvent;
        public delegate void OnHitNPCDelegate(Player player, Item item, NPC target, int damage, float knockback, bool crit);
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            OnHitNPCEvent?.Invoke(player, item, target, damage, knockback, crit);
            sendHitPacket();
        }


        /// <summary>
        /// Use this event for projectile hitting npcs for situations where a projectile should be owned by a player.
        /// Set StarlightPlayer.shouldSendHitPacket to true to sync if this has an effect for multiplayer.
        /// </summary>
        public static event OnHitNPCWithProjDelegate OnHitNPCWithProjEvent;
        public delegate void OnHitNPCWithProjDelegate(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit);
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            OnHitNPCWithProjEvent?.Invoke(player, proj, target, damage, knockback, crit);
            sendHitPacket();
        }

        public delegate void NaturalLifeRegenDelegate(Player player, ref float regen);
        public static event NaturalLifeRegenDelegate NaturalLifeRegenEvent;
        public override void NaturalLifeRegen(ref float regen)
        {
            NaturalLifeRegenEvent?.Invoke(player, ref regen);
        }

        public delegate void PostUpdateDelegate(Player player);
        public static event PostUpdateDelegate PostUpdateEvent;

        public delegate void PostDrawDelegate(Player player, SpriteBatch spriteBatch);
        public static event PostDrawDelegate PostDrawEvent;
		public void PostDraw(Player player, SpriteBatch spriteBatch)
		{
			PostDrawEvent?.Invoke(player, Main.spriteBatch);
		}

        public delegate void PreDrawDelegate(Player player, SpriteBatch spriteBatch);
        public static event PreDrawDelegate PreDrawEvent;
        public void PreDraw(Player player, SpriteBatch spriteBatch)
        {
            PreDrawEvent?.Invoke(player, Main.spriteBatch);
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
                    result &= del(player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
                }
                return result;
            }
            return true;
        }
    }
}
