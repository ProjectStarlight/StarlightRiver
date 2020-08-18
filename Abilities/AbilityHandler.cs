using Microsoft.Xna.Framework;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Abilities
{
    public partial class AbilityHandler : ModPlayer
    {
        private Dictionary<Type, Ability> unlockedAbilities = new Dictionary<Type, Ability>();
        private HashSet<StaminaShardId> shards = new HashSet<StaminaShardId>();

        public Ability ActiveAbility { get; internal set; }

        //The players stamina stats.
        public float StaminaMax => StaminaMaxDefault + StaminaMaxBonus;
        public float StaminaMaxDefault => 3 + shards.Count / shardsPerVessel;
        public float StaminaMaxBonus 
        { 
            get => staminaMaxBonus; 
            set 
            { 
                staminaMaxBonus = Math.Max(value, -StaminaMaxDefault); // Can't have less than 0 max hp...
                Stamina = stamina; // Update Stamina property setter safely
            }
        }
        public float Stamina 
        { 
            get => stamina; 
            set => stamina = MathHelper.Clamp(value, 0, StaminaMax); 
        }
        public float StaminaRegenRate { get; set; }

        private const int staminaRegenCDMax = 180;
        private const int shardsPerVessel = 3;
        private int staminaRegenCD;
        private float stamina;
        private float staminaMaxBonus;

        public override TagCompound Save()
        {
            return new TagCompound
            {
                [nameof(shards)] = shards.ToList(),
                [nameof(unlockedAbilities)] = unlockedAbilities.Select(t => t.GetType().AssemblyQualifiedName).ToList()
            };
        }

        public override void Load(TagCompound tag)
        {
            var shardsTemp = tag.GetList<StaminaShardId>(nameof(shards));
            shards = new HashSet<StaminaShardId>(shardsTemp);

            var abilitiesTemp = tag.GetList<string>(nameof(unlockedAbilities));
            unlockedAbilities = new Dictionary<Type, Ability>();
            foreach (var item in abilitiesTemp)
            {
                Type t = Type.GetType(item);
                unlockedAbilities[t] = Activator.CreateInstance(t) as Ability;
            }
        }

        /// <summary>
        /// Unlocks the ability type for the player.
        /// </summary>
        public void Unlock<T>() where T : Ability, new()
        {
            // Ensure we don't unlock the same thing twice
            if (!unlockedAbilities.ContainsKey(typeof(T)))
            {
                unlockedAbilities[typeof(T)] = new T();
            }
        }
        /// <summary>
        /// Re-locks the ability type for the player.
        /// </summary>
        /// <returns>Whether the op was successful.</returns>
        public bool Lock<T>() where T : Ability => unlockedAbilities.Remove(typeof(T));
        /// <summary>
        /// Re-locks the ability type for the player.
        /// </summary>
        /// <param name="value">The matching ability, if any.</param>
        /// <returns>If an ability was found.</returns>
        public bool IsUnlocked<T>(out Ability value) => unlockedAbilities.TryGetValue(typeof(T), out value);

        public override void ResetEffects()
        {
            //Resets the player's stamina to prevent issues with gaining infinite stamina or stamina regeneration.
            staminaMaxBonus = 0;
            StaminaRegenRate = 0;

            if (ActiveAbility != null)
            {
                // The player cant use items while casting an ability.
                player.noItems = true;
                player.noBuilding = true;
            }
        }

        public override void UpdateDead()
        {
            ActiveAbility = null;
        }

        public override void OnRespawn(Player player)
        {
            Stamina = StaminaMax;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Beyah
            foreach (var item in unlockedAbilities.Values)
            {
                if (item.HotkeyMatch(triggersSet) && item.Available)
                {
                    item.Activate();
                    break;
                }
            }
        }

        public override void PreUpdate()
        {
            foreach (Ability ability in unlockedAbilities.Values)
            {
                ability.Update();
            }
            
            if (ActiveAbility != null)
            {
                ActiveAbility.UpdateActive();
                unlockedAbilities[ActiveAbility.GetType()] = ActiveAbility;
            }

            if (ActiveAbility != null)
            {
                player.velocity.Y += 0.01f; //Required to ensure that the game never thinks we hit the ground when using an ability. Thanks redcode!

                player.rocketRelease = true;
                player.fallStart = (int)player.Center.Y;
                player.fallStart2 = (int)player.Center.Y;
                player.rocketTimeMax = 0;
                player.wingTimeMax = 0;

                staminaRegenCD = staminaRegenCDMax;
            }
            else
            {
                if (staminaRegenCD > 1)
                {
                    staminaRegenCD--;
                }
                Stamina += StaminaRegenRate * staminaRegenCD;
            }
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            ActiveAbility?.ModifyDrawLayers(layers);
        }
    }
}