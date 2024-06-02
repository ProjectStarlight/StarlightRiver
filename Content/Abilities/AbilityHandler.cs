using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Abilities
{
	public class AbilityHandler : ModPlayer, IOrderedLoadable
	{
		private const int SHARDS_PER_VESSEL = 3;

		private InfusionItem[] infusions = new InfusionItem[Infusion.InfusionSlots];
		public Dictionary<Type, Ability> unlockedAbilities = new();

		private float stamina;
		private float staminaMaxBonus;
		private int staminaRegenCD;

		private Ability activeAbility;
		private Ability nextAbility;

		public float Priority => 1;

		/// <summary>
		/// The players effective maximum stamina
		/// </summary>
		public float StaminaMax => StaminaMaxDefault + StaminaMaxBonus;
		public float StaminaMaxDefault => Shards.Count / SHARDS_PER_VESSEL + unlockedAbilities.Count;

		/// <summary>
		/// The rate at which the player regenerates stamina. One point is equal to 0.05 stamina per second.
		/// </summary>
		public int StaminaRegenRate { get; set; }

		/// <summary>
		/// The maximum amount of infusions the player can equip
		/// </summary>
		public int InfusionLimit { get; set; } = 0;

		/// <summary>
		/// Global stamina cost multiplier. Used to increase or decrease the cost of ALL abilities by a multiplicative value
		/// </summary>
		public float StaminaCostMultiplier { get; set; }

		/// <summary>
		/// Global stamina cost flat modifier. Used to increase or decrease the cost of ALL abilities by a flat value
		/// </summary>
		public float StaminaCostBonus { get; set; }

		/// <summary>
		/// The amount of stamina vessel shards the player has
		/// </summary>
		public int ShardCount => Shards.Count;

		/// <summary>
		/// If the player has unlocked any abilties or not. Used for UI visibility
		/// </summary>
		public bool AnyUnlocked => unlockedAbilities.Count > 0;

		/// <summary>
		/// The stamina vessel shards the player has collected, as each is unique
		/// </summary>
		public ShardSet Shards { get; private set; } = new ShardSet();

		/// <summary>
		/// The player's currently active ability
		/// </summary>
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

		/// <summary>
		/// A flat modifier to the player's max stamina. Can be used to increase or decrease it, but not lower than zero.
		/// </summary>
		public float StaminaMaxBonus
		{
			get => staminaMaxBonus;
			set =>
				// Can't have less than 0 max sp.
				staminaMaxBonus = Math.Max(value, -StaminaMaxDefault);
		}

		/// <summary>
		/// The player's base stamina, should only be effected by ability and vessel unlocks.
		/// </summary>
		public float Stamina
		{
			get => stamina;
			// Can't have less than 0 or more than max stamina.
			set => stamina = MathHelper.Clamp(value, 0, StaminaMax);
		}

		//for some reason without specifically setting these values to zero with cloneNewInstances => false and contructor,
		////on a server if someone unlocks or modifies these it will impact newly created characters from then on for that instance
		protected override bool CloneNewInstances => false;

		public AbilityHandler()
		{
			infusions = new InfusionItem[Infusion.InfusionSlots];
			unlockedAbilities = new Dictionary<Type, Ability>();
		}

		public override void Load()
		{
			StarlightPlayer.PostDrawEvent += PostDrawAbility;
		}

		public override void Unload() { }

		/// <summary>
		/// Internal method for handling an ability unlock, used by the publically exposed generic methods
		/// </summary>
		/// <param name="t"></param>
		/// <param name="ability"></param>
		private void Unlock(Type t, Ability ability)
		{
			unlockedAbilities[t] = ability;
			ability.User = this;
		}

		/// <summary>
		/// Internal method for checking and retrieving an infusion if the player has it on.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="infusion"></param>
		/// <returns></returns>
		private bool TryMatchInfusion(Type t, out InfusionItem infusion)
		{
			infusion = infusions.FirstOrDefault(i => i?.AbilityType == t);
			return infusion != null;
		}

		/// <summary>
		/// Re-locks an ability for the player. Typically only used for debugging purposes.
		/// </summary>
		/// <typeparam name="T">The type of the ability to lock</typeparam>
		public void Lock<T>() where T : Ability
		{
			unlockedAbilities.Remove(typeof(T));
		}

		/// <summary>
		/// Unlocks the ability type for the Player.
		/// </summary>
		public void Unlock<T>() where T : Ability, new()
		{
			// Ensure we don't unlock the same thing twice
			if (!unlockedAbilities.ContainsKey(typeof(T)))
				Unlock(typeof(T), new T());
		}

		/// <summary>
		/// Tries to get an unlocked ability from the Player.
		/// </summary>
		/// <typeparam name="T">The type of ability.</typeparam>
		/// <param name="value">The ability.</param>
		/// <returns>Success or not.</returns>
		public bool GetAbility<T>(out T value) where T : Ability
		{
			bool r = unlockedAbilities.TryGetValue(typeof(T), out Ability a);
			value = a as T;
			return r;
		}

		public bool GetAbility(Type type, out Ability ability)
		{
			return unlockedAbilities.TryGetValue(type, out ability);
		}

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
		/// <param name="Item">The Item to add.</param>
		/// <returns>If the add was successful.</returns>
		public bool SetInfusion(InfusionItem Item, int slot)
		{
			// Safety check
			if (!CanSetInfusion(Item))
				return false;

			// Null check
			if (Item == null)
			{
				infusions[slot] = null;
				return true;
			}

			// General use case
			Item newItem = Item.Item.Clone();
			newItem.SetDefaults(Item.Item.type);
			newItem.playerIndexTheItemIsReservedFor = Player.whoAmI;
			infusions[slot] = newItem.ModItem as InfusionItem;

			return true;
		}

		/// <summary>
		/// Checks if the infusion can be added.
		/// </summary>
		/// <param name="Item">The Item.</param>
		/// <returns>Whether the Item can be added.</returns>
		public bool CanSetInfusion(InfusionItem Item)
		{
			if (Item == null)
				return true;

			if (!Item.Equippable)
				return false;

			for (int i = 0; i < infusions.Length; i++)
			{
				InfusionItem infusion = infusions[i];
				if (infusion is null)
					continue;

				if (Item.AbilityType != null && Item.AbilityType == infusion.AbilityType ||
					Item.GetType() == infusion.GetType())
				{
					return false;
				}
			}

			return infusions.Count(n => n != null) < InfusionLimit;
		}

		/// <summary>
		/// Retrieves the infusion item for the given infusion slot
		/// </summary>
		/// <param name="slot">The index of the slot to retrieve</param>
		/// <returns>The InfusionItem ModItem of the infusion in the given slot, or null if one is not there.</returns>
		public InfusionItem GetInfusion(int slot)
		{
			return slot < 0 || slot >= infusions.Length ? null : infusions[slot];
		}

		/// <summary>
		/// Sets the player's stamina regen delay, in ticks. Anything larger than 60 will prevent the player from regenerating stamina untill the time minus 60 expires.
		/// </summary>
		/// <param name="cooldownTicks">How long the player's stamina delay is set for</param>
		public void SetStaminaRegenCD(int cooldownTicks)
		{
			staminaRegenCD = Math.Max(staminaRegenCD, cooldownTicks);
		}

		public override void SaveData(TagCompound tag)
		{
			tag[nameof(Shards)] = Shards.ToList();
			tag[nameof(unlockedAbilities)] = unlockedAbilities.Keys.Select(t => t.FullName).ToList();
			tag[nameof(infusions)] = infusions.Where(t => t != null).Select(t => t.Item).ToList();
			tag[nameof(InfusionLimit)] = InfusionLimit;
		}

		public override void LoadData(TagCompound tag)
		{
			Shards = new ShardSet();
			unlockedAbilities = new Dictionary<Type, Ability>();
			infusions = new InfusionItem[Infusion.InfusionSlots];
			InfusionLimit = 1;
			try
			{
				// Load shards
				IList<int> shardsTemp = tag.GetList<int>(nameof(Shards));
				foreach (int Item in shardsTemp)
					Shards.Add(Item);

				// Load unlocked abilities and init them
				IList<string> abilitiesTemp = tag.GetList<string>(nameof(unlockedAbilities));
				foreach (string Item in abilitiesTemp)
				{
					Type t = typeof(Ability).Assembly.GetType(Item);
					if (t != null)
						Unlock(t, Activator.CreateInstance(t) as Ability);
				}

				// Load infusions
				IList<Item> infusionsTemp = tag.GetList<Item>(nameof(infusions));
				for (int i = 0; i < infusionsTemp.Count; i++)
					infusions[i] = infusionsTemp[i].ModItem as InfusionItem;

				// Load max infusions
				InfusionLimit = tag.GetInt(nameof(InfusionLimit));
			}
			catch (Exception e)
			{
				StarlightRiver.Instance.Logger.Debug("handled error loading Player: " + e);
			}
		}

		public override void ResetEffects()
		{
			//Resets the Player's stamina to prevent issues with gaining infinite stamina or stamina regeneration.
			staminaMaxBonus = 0;
			StaminaRegenRate = 5;
			StaminaCostMultiplier = 1;
			StaminaCostBonus = 0;

			if (ActiveAbility != null)
			{
				// The Player cant use Items while casting an ability.               
				Player.noItems = true;
				Player.noBuilding = true;
			}
		}

		public override void UpdateDead()
		{
			ActiveAbility = null;
		}

		public override void OnRespawn()
		{
			Stamina = StaminaMax;
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			// Check infusions FIRST so they take precedence
			foreach (InfusionItem infusion in infusions)
			{
				if (infusion == null)
					continue;

				// If any of the infusions match, they take precedent. This prevents "falling through" to the default ability
				if (infusion.ability != null && infusion.ability.HotKeyMatch(triggersSet, StarlightRiver.Instance.AbilityKeys))
				{
					if (infusion.ability.Available)
						infusion.ability.Activate(this);

					return;
				}
			}

			// Then check the default abilities
			foreach (Ability ability in unlockedAbilities.Values)
			{
				if (ability.Available && ability.HotKeyMatch(triggersSet, StarlightRiver.Instance.AbilityKeys))
				{
					ability.Activate(this);
					break;
				}
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
				Player.velocity.Y += 0.01f;

				// Disable wings and rockets temporarily
				Player.canRocket = false;
				Player.rocketBoots = -1;
				Player.wings = -1;

				SetStaminaRegenCD(60);
			}
			else
			{
				UpdateStaminaRegen();
			}

			// To ensure fusions always have their owner set to a valid Player.
			for (int i = 0; i < infusions.Length; i++)
			{
				if (infusions[i] != null)
					infusions[i].Item.playerIndexTheItemIsReservedFor = Player.whoAmI;
			}
		}

		/// <summary>
		/// Handles updating the player's stamina values based on their regeneration stat
		/// </summary>
		private void UpdateStaminaRegen()
		{
			const int cooldownSmoothing = 10;

			// Faster regen while not moving much
			if (Player.velocity.LengthSquared() > 1)
				SetStaminaRegenCD(cooldownSmoothing);

			// Decrement cooldown
			if (staminaRegenCD > 0)
				staminaRegenCD--;

			// Regen stamina at a speed inversely proportional to the smoothed cooldown
			Stamina += StaminaRegenRate * 0.05f / 60f * (1 - staminaRegenCD / 60f);
		}

		/// <summary>
		/// This handles running the updates of the active ability/infusion, and properly dealing with those that have
		/// expired already.
		/// </summary>
		private void UpdateAbilities()
		{
			var called = new HashSet<Ability>();

			foreach (Ability ability in unlockedAbilities.Values)
			{
				called.Add(ability);
			}

			// Update infusions
			foreach (InfusionItem infusion in infusions)
			{
				if (infusion == null)
					continue;

				if (infusion.ability != null)
				{
					called.Add(infusion.ability);
					called.Remove(infusion.BaseAbility);
				}
			}

			// Update abilities unaffected by infusions
			foreach (Ability ability in called)
			{
				ability.User = this;
				ability.UpdateFixed();
			}

			// Update active ability if unaffected by an infusion
			if (ActiveAbility != null && called.Contains(ActiveAbility))
			{
				ActiveAbility.UpdateActive();

				if (Main.netMode != NetmodeID.Server)
					ActiveAbility.UpdateActiveEffects();

				if (Main.netMode != NetmodeID.Server && Player == Main.LocalPlayer)
					NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.LocalPlayer.whoAmI);
			}
		}

		/// <summary>
		/// This handles activation and deactivation of the currently active ability
		/// </summary>
		private void UpdateActiveAbilityHooks()
		{
			// Call the current ability's deactivation hooks
			if (activeAbility != null)
			{
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
				nextAbility.OnActivate();
			}
		}

		public override void OnEnterWorld()
		{
			var packet = new AbilityProgress(Player.whoAmI, Player.GetHandler());
			packet.Send();
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			ActiveAbility?.ModifyDrawInfo(ref drawInfo);
		}

		/// <summary>
		/// This handles calling the visual effect hooks of active abilities
		/// </summary>
		/// <param name="Player"></param>
		/// <param name="spriteBatch"></param>
		public void PostDrawAbility(Player Player, SpriteBatch spriteBatch)
		{
			var called = new HashSet<Ability>();

			foreach (Ability ability in Player.GetHandler().unlockedAbilities.Values)
			{
				called.Add(ability);
			}

			// Update infusions
			foreach (InfusionItem infusion in Player.GetHandler().infusions)
			{
				if (infusion == null)
					continue;

				if (infusion.ability != null)
				{
					called.Add(infusion.ability);
					called.Remove(infusion.BaseAbility);
				}
			}

			foreach (Ability ability in called)
				ability.DrawActiveEffects(spriteBatch);
		}
	}
}