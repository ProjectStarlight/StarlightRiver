using Microsoft.Xna.Framework;
using Newtonsoft.Json.Serialization;
using StarlightRiver.GUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Abilities
{
    public partial class AbilityHandler : ModPlayer
    {
        // The player's active ability.
        public Ability ActiveAbility
        {
            get => activeAbility;
            internal set => ActivateAbility(value);
        }

        private void ActivateAbility(Ability value)
        {
            // Ignore dupes
            if (ReferenceEquals(value, activeAbility))
            {
                return;
            }
            // If we have enough stamina
            if (value is null || Stamina > value.ActivationCost(this))
            {
                // Fire ability hooks
                if (activeAbility != null)
                {
                    if (TryMatchInfusion(activeAbility.GetType(), out var infusion))
                        infusion.OnExit();
                    else
                        activeAbility.OnExit();
                }
                activeAbility = value;
                // Fire more ability hooks and update stamina
                if (activeAbility != null)
                {
                    // Stamina
                    activeAbility.User = this;
                    Stamina -= activeAbility.ActivationCost(this);
                    activeAbility.ActivationCostBonus = 0;

                    // Hooks
                    if (TryMatchInfusion(activeAbility.GetType(), out var infusion))
                        infusion.OnActivate();
                    else
                        activeAbility.OnActivate();
                }
            }
        }

        // The player's stamina stats.
        public float StaminaMax => StaminaMaxDefault + StaminaMaxBonus;
        public float StaminaMaxDefault => 1 + Shards.Count / shardsPerVessel + unlockedAbilities.Count;
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
        public int InfusionLimit { get; set; } = 1;

        public float StaminaCostMultiplier { get; set; }
        public float StaminaCostBonus { get; set; }

        public int ShardCount => Shards.Count;
        public bool AnyUnlocked => unlockedAbilities.Count > 0;

        // Some constants.
        private const int shardsPerVessel = 3;

        public ShardSet Shards { get; private set; } = new ShardSet();

        // Internal-only information.

        private InfusionItem[] infusions = new InfusionItem[GUI.Infusion.InfusionSlots];
        private Dictionary<Type, Ability> unlockedAbilities = new Dictionary<Type, Ability>();
        private int staminaRegenCD;
        private float stamina;
        private float staminaMaxBonus;
        private Ability activeAbility;

        private void Unlock(Type t, Ability ability)
        {
            unlockedAbilities[t] = ability;
            ability.User = this;
        }

        private bool TryMatchInfusion(Type t, out InfusionItem infusion)
        {
            infusion = infusions.FirstOrDefault(i => i?.AbilityType == t);
            return infusion != null;
        }

        /// <summary>
        /// Unlocks the ability type for the player.
        /// </summary>
        public void Unlock<T>() where T : Ability, new()
        {
            // Ensure we don't unlock the same thing twice
            if (!unlockedAbilities.ContainsKey(typeof(T)))
            {
                Unlock(typeof(T), new T());
            }
        }

        /// <summary>
        /// Tries to get an unlocked ability from the player.
        /// </summary>
        /// <typeparam name="T">The type of ability.</typeparam>
        /// <param name="value">The ability.</param>
        /// <returns>Success or not.</returns>
        public bool GetAbility<T>(out T value) where T : Ability
        {
            bool r = unlockedAbilities.TryGetValue(typeof(T), out var a);
            value = a as T;
            return r;
        }

        public bool GetAbility(Type type, out Ability ability) => unlockedAbilities.TryGetValue(type, out ability);

        /// <summary>
        /// Checks if the given ability type is unlocked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Unlocked<T>() where T : Ability
        {
            return GetAbility<T>(out _);
        }

        /// <summary>
        /// Checks if the given ability type is unlocked.
        /// </summary>
        public bool Unlocked(Type type)
        {
            return unlockedAbilities.ContainsKey(type);
        }

        /// <summary>
        /// Tries to add the matching infusion type.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>If the add was successful.</returns>
        public bool SetInfusion(InfusionItem item, int slot)
        {
            // Safety check
            if (!CanSetInfusion(item))
                return false;

            // Null check
            if (item == null)
            {
                infusions[slot] = null;
                return true;
            }

            // General use case
            var newItem = item.item.Clone();
            newItem.SetDefaults(item.item.type);
            infusions[slot] = newItem.modItem as InfusionItem;
            infusions[slot].Player = player;

            return true;
        }

        /// <summary>
        /// Checks if the infusion can be added.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Whether the item can be added.</returns>
        public bool CanSetInfusion(InfusionItem item)
        {
            if (item == null) 
                return true;

            foreach (var infusion in infusions)
            {
                if (infusion is null) continue;

                if (item.AbilityType != null && item.AbilityType == infusion.AbilityType ||
                    item.GetType() == infusion.GetType())
                    return false;
            }
            return true;
        }

        public InfusionItem GetInfusion(int slot) => slot < 0 || slot >= infusions.Length ? null : infusions[slot];

        public void SetStaminaRegenCD(int cooldownTicks) => staminaRegenCD = Math.Max(staminaRegenCD, cooldownTicks);

        public override TagCompound Save()
        {
            return new TagCompound
            {
                [nameof(Shards)] = Shards.ToList(),
                [nameof(unlockedAbilities)] = unlockedAbilities.Keys.Select(t => t.FullName).ToList(),
                [nameof(infusions)] = infusions.Where(t => t != null).Select(t => t.item).ToList(),
                [nameof(InfusionLimit)] = InfusionLimit
            };
        }

        public override void Load(TagCompound tag)
        {
            Shards = new ShardSet();
            unlockedAbilities = new Dictionary<Type, Ability>();
            infusions = new InfusionItem[GUI.Infusion.InfusionSlots];
            InfusionLimit = 1;
            try
            {
                // Load shards
                var shardsTemp = tag.GetList<int>(nameof(Shards));
                foreach (var item in shardsTemp)
                {
                    Shards.Add(item);
                }

                // Load unlocked abilities and init them
                var abilitiesTemp = tag.GetList<string>(nameof(unlockedAbilities));
                foreach (var item in abilitiesTemp)
                {
                    var t = typeof(Ability).Assembly.GetType(item);
                    if (t != null)
                        Unlock(t, Activator.CreateInstance(t) as Ability);
                }

                // Load infusions
                var infusionsTemp = tag.GetList<Item>(nameof(infusions));
                for (int i = 0; i < infusionsTemp.Count; i++)
                {
                    infusions[i] = infusionsTemp[i].modItem as InfusionItem;
                    if (infusions[i] != null)
                        infusions[i].Player = player;
                }

                // Load max infusions
                InfusionLimit = tag.GetInt(nameof(InfusionLimit));
            }
            catch
            {

            }
        }

        public override void ResetEffects()
        {
            //Resets the player's stamina to prevent issues with gaining infinite stamina or stamina regeneration.
            staminaMaxBonus = 0;
            StaminaRegenRate = 1 / 60f * 2; // stamina per tick = 1 / 60f * (stamina per second)
            StaminaCostMultiplier = 1;
            StaminaCostBonus = 0;

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
                if (item.Available && item.HotKeyMatch(triggersSet, StarlightRiver.Instance.AbilityKeys))
                {
                    item.Activate(this);
                    break;
                }
            }
        }

        public override void PreUpdate()
        {
            UpdateAbilities();

            if (ActiveAbility != null)
            {
                // Jank
                player.velocity.Y += 0.01f;

                // Disable wings and rockets temporarily
                player.canRocket = false;
                player.rocketBoots = -1;
                player.wings = -1;

                SetStaminaRegenCD(200);
            }
            else
            {
                UpdateStaminaRegen();
            }
        }

        private void UpdateStaminaRegen()
        {
            const int regenSmoothing = 10;

            // Faster regen while not moving much
            if (player.velocity.LengthSquared() > 1)
            {
                SetStaminaRegenCD(regenSmoothing);
            }

            // Regen stamina
            if (staminaRegenCD > 0)
            {
                staminaRegenCD--;
            }
            Stamina += StaminaRegenRate / ((staminaRegenCD / (float)regenSmoothing) + 1);
        }

        private void UpdateAbilities()
        {
            var called = new HashSet<Ability>();

            // Update infusions
            foreach (var infusion in infusions)
            {
                if (infusion == null) continue;
                infusion.UpdateFixed();
                if (infusion.Ability != null)
                {
                    if (infusion.AbilityType == ActiveAbility?.GetType())
                    {
                        infusion.UpdateActive();
                        if (Main.netMode != NetmodeID.Server)
                            infusion.UpdateActiveEffects();
                    }
                    called.Add(infusion.Ability);
                }
            }

            // Cut out previously called abilities
            called.SymmetricExceptWith(unlockedAbilities.Values);

            // Iterate abilities unaffected by infusions
            foreach (var ability in called)
            {
                ability.UpdateFixed();
            }

            // Update active ability if unaffected by an infusion
            if (ActiveAbility != null && called.Contains(ActiveAbility))
            {
                var temp = ActiveAbility;
                temp.UpdateActive();
                if (Main.netMode != NetmodeID.Server)
                    temp.UpdateActiveEffects();
            }
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            ActiveAbility?.ModifyDrawLayers(layers);
        }
    }
}