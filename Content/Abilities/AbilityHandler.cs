using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Abilities
{
	public class AbilityHandler : ModPlayer, ILoadable
    {
        // The player's active ability.
        public Ability ActiveAbility
        {
            get => activeAbility;
            set
            {
                // Cache the next ability to be updated next update.
                nextAbility = value;

                // Call the ability update hooks immediately.
                UpdateActiveAbilityHooks();
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
                // Can't have less than 0 max hp.
                staminaMaxBonus = Math.Max(value, -StaminaMaxDefault);
            }
        }
        public float Stamina
        {
            get => stamina;
            // Can't have less than 0 or more than max stamina.
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

        private InfusionItem[] infusions = new InfusionItem[Infusion.InfusionSlots];
        public Dictionary<Type, Ability> unlockedAbilities = new Dictionary<Type, Ability>();
        private int staminaRegenCD;
        private float stamina;
        private float staminaMaxBonus;
        private Ability activeAbility;
        private Ability nextAbility;

        public float Priority => 1;

        //for some reason without specifically setting these values to zero with cloneNewInstances => false and contructor,
        ////on a server if someone unlocks or modifies these it will impact newly created characters from then on for that instance
        public override bool CloneNewInstances => false;
        public AbilityHandler()
        {
            infusions = new InfusionItem[Infusion.InfusionSlots];
            unlockedAbilities = new Dictionary<Type, Ability>();
        }

        public void Load()
		{
            StarlightPlayer.PostDrawEvent += PostDrawAbility;
		}

        public void Unload() { }

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

        public void Lock<T>() where T : Ability => unlockedAbilities.Remove(typeof(T));

        /// <summary>
        /// Unlocks the ability type for the player.
        /// </summary>
        public void Unlock<T>() where T : Ability, new()
        {
            // Ensure we don't unlock the same thing twice
            if (!unlockedAbilities.ContainsKey(typeof(T)))
                Unlock(typeof(T), new T());
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
            newItem.owner = player.whoAmI;
            infusions[slot] = newItem.modItem as InfusionItem;

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

            for (int i = 0; i < infusions.Length; i++)
            {
                InfusionItem infusion = infusions[i];
                if (infusion is null) continue;

                if (item.AbilityType != null && item.AbilityType == infusion.AbilityType ||
                    item.GetType() == infusion.GetType())
                    return false;
            }
            return true;
        }

        public InfusionItem GetInfusion(int slot) => slot < 0 || slot >= infusions.Length ? null : infusions[slot];

        public void SetStaminaRegenCD(int cooldownTicks) => staminaRegenCD = Math.Max(staminaRegenCD, cooldownTicks);

		public override void SendClientChanges(ModPlayer clientPlayer) //TODO: Implement ablity packet
		{
			base.SendClientChanges(clientPlayer);
		}

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
            infusions = new InfusionItem[Infusion.InfusionSlots];
            InfusionLimit = 1;
            try
            {
                // Load shards
                var shardsTemp = tag.GetList<int>(nameof(Shards));
                foreach (var item in shardsTemp)
                    Shards.Add(item);

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
                    infusions[i] = infusionsTemp[i].modItem as InfusionItem;

                // Load max infusions
                InfusionLimit = tag.GetInt(nameof(InfusionLimit));
            }
            catch (Exception e)
            {
                StarlightRiver.Instance.Logger.Debug("handled error loading player: " + e);
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
            foreach (var ability in unlockedAbilities.Values)
                if (ability.Available && ability.HotKeyMatch(triggersSet, StarlightRiver.Instance.AbilityKeys))
                {
                    ability.Activate(this);
                    break;
                }
        }

        public override void PreUpdate()
        {
            // Set new active ability per-tick
            activeAbility = nextAbility;

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
                UpdateStaminaRegen();

            // To ensure fusions always have their owner set to a valid player.
            for (int i = 0; i < infusions.Length; i++)
                if (infusions[i] != null)
                    infusions[i].item.owner = player.whoAmI;
        }
      
        private void UpdateStaminaRegen()
        {
            const int cooldownSmoothing = 10;

            // Faster regen while not moving much
            if (player.velocity.LengthSquared() > 1)
                SetStaminaRegenCD(cooldownSmoothing);

            // Decrement cooldown
            if (staminaRegenCD > 0)
                staminaRegenCD--;

            // Regen stamina at a speed inversely proportional to the smoothed cooldown
            Stamina += StaminaRegenRate / (staminaRegenCD / (float)cooldownSmoothing + 1);
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

            // Update abilities unaffected by infusions
            foreach (var ability in called)
                ability.UpdateFixed();

            // Update active ability if unaffected by an infusion
            if (ActiveAbility != null && called.Contains(ActiveAbility))
            {
                ActiveAbility.UpdateActive();
                ActiveAbility.UpdateActiveEffects();

                if (Main.netMode != NetmodeID.Server && player == Main.LocalPlayer)                 
                    NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.LocalPlayer.whoAmI);              
            }
        }

        private void UpdateActiveAbilityHooks()
        {
            // Call the current ability's deactivation hooks
            if (activeAbility != null)
            {
                if (TryMatchInfusion(activeAbility.GetType(), out var infusion))
                    infusion.OnExit();
                else
                    activeAbility.OnExit();
                activeAbility.Reset();
            }

            // Call the new current ability's activation hooks, and apply stamina cost if new ability is real
            if (nextAbility != null)
            {
                // Stamina cost
                nextAbility.User = this;
                Stamina -= nextAbility.ActivationCost(this);
                nextAbility.ActivationCostBonus = 0;

                // Hooks
                if (TryMatchInfusion(nextAbility.GetType(), out var infusion))
                    infusion.OnActivate();
                else
                    nextAbility.OnActivate();
            }
        }

		public override void OnEnterWorld(Player player)
		{
            AbilityProgress packet = new AbilityProgress(player.whoAmI, player.GetHandler());
            packet.Send();
        }

		public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            ActiveAbility?.ModifyDrawLayers(layers);
        }

        public void PostDrawAbility(Player player, SpriteBatch spriteBatch)
		{
            var called = new HashSet<Ability>();

            foreach (var infusion in player.GetHandler().infusions)
            {
                if (infusion == null) continue;
                infusion.UpdateFixed();

                if (infusion.Ability != null)
                    called.Add(infusion.Ability);
            }

            called.SymmetricExceptWith(player.GetHandler().unlockedAbilities.Values);

            foreach (var ability in called)
                ability.DrawActiveEffects(spriteBatch);
        }
    }
}