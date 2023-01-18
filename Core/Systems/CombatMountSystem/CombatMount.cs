using StarlightRiver.Content.Prefixes.CombatMountPrefixes;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Utilities;

namespace StarlightRiver.Core.Systems.CombatMountSystem
{
	public abstract class CombatMount
	{
		public int primaryAttackTimer;
		public int secondaryAbilityTimer;

		public int primaryCooldownTimer;
		public int secondaryCooldownTimer;

		/// <summary>
		/// The total use time of the primary attack is multiplied by this. Lower values are faster.
		/// </summary>
		public float primarySpeedMultiplier = 1;
		/// <summary>
		/// The cooldown of the secondary ability is multiplied by this. Lower values make the cooldown faster.
		/// </summary>
		public float secondaryCooldownSpeedMultiplier = 1;
		/// <summary>
		/// The movement of the mount is/should be effected by this value. A higher value should represent faster/more agile movement.
		/// </summary>
		public float moveSpeedMultiplier = 1;
		/// <summary>
		/// The base amount of damage this mount does. The primary and secondary effects may be effected by this differently.
		/// </summary>
		public int damageCoefficient;

		/// <summary>
		/// The base amount of time it takes for the primary attacks to complete.
		/// </summary>
		public int primarySpeedCoefficient;
		/// <summary>
		/// The base amount of time it takes for the secondary ability to complete.
		/// </summary>
		public int secondarySpeedCoefficient;

		/// <summary>
		/// The base amount of time between primary attacks.
		/// </summary>
		public int primaryCooldownCoefficient;
		/// <summary>
		/// The base amount of time between secondary ability uses.
		/// </summary>
		public int secondaryCooldownCoefficient;

		/// <summary>
		/// If the primary attack should have autoReuse. Same as Item.autoReuse.
		/// </summary>
		public bool autoReuse;

		/// <summary>
		/// Gets the total time a primary action should take after modifiers are applied.
		/// </summary>
		public int MaxPrimaryTime => (int)Math.Round(primarySpeedCoefficient * primarySpeedMultiplier);
		/// <summary>
		/// Gets the total time for a single primary action cycle. (time + cooldown)
		/// </summary>
		public int MaxPrimaryCooldown => MaxPrimaryTime + primaryCooldownCoefficient;
		/// <summary>
		/// gets the total time between secondary action uses. (time + cooldown * multiplier)
		/// </summary>
		public int MaxSecondaryCooldown => (int)Math.Round(secondaryCooldownCoefficient * secondaryCooldownSpeedMultiplier + secondarySpeedCoefficient);

		public virtual string PrimaryIconTexture => AssetDirectory.Debug;
		public virtual string SecondaryIconTexture => AssetDirectory.Debug;

		public void MountUp(Player player)
		{
			SetDefaults();
			player.GetModPlayer<CombatMountPlayer>().activeMount = this;
		}

		public void ResetStats()
		{
			primarySpeedMultiplier = 1;
			secondaryCooldownSpeedMultiplier = 1;
			moveSpeedMultiplier = 1;
			damageCoefficient = 10;
			primarySpeedCoefficient = 20;
			secondarySpeedCoefficient = 20;
			primaryCooldownCoefficient = 20;
			secondaryCooldownCoefficient = 20;
			autoReuse = false;
		}

		public virtual void SetDefaults()
		{

		}

		/// <summary>
		/// Update function called after both primary and secondary actions run. Can be used for custom physics and applying movement speed stat properly, along with passive effects and stat changes.
		/// </summary>
		/// <param name="player"></param>
		public virtual void PostUpdate(Player player)
		{

		}

		public void StartPrimaryAction(Player player)
		{
			primaryAttackTimer = MaxPrimaryTime;
			primaryCooldownTimer = primaryCooldownCoefficient + primaryAttackTimer;
			OnStartPrimaryAction(player);
		}

		/// <summary>
		/// Effects which occur when the primary attack is first triggered.
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnStartPrimaryAction(Player player)
		{

		}

		/// <summary>
		/// Effects which occur over the course of the entire primary attack. Use MaxPrimaryTime to compare against the max value for the timer parameter.
		/// </summary>
		/// <param name="timer"></param>
		/// <param name="player"></param>
		public virtual void PrimaryAction(int timer, Player player)
		{

		}

		public void StartSecondaryAction(Player player)
		{
			secondaryAbilityTimer = secondarySpeedCoefficient;
			secondaryCooldownTimer = (int)(secondaryCooldownCoefficient * secondaryCooldownSpeedMultiplier) + secondaryAbilityTimer;
			OnStartSecondaryAction(player);
		}

		/// <summary>
		/// Effects which occur when the secondary ability is first triggered.
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnStartSecondaryAction(Player player)
		{

		}

		/// <summary>
		/// Effects which occur over the course of the entire secondary attack. Use secondarySpeedCoefficient to compare against the max value of timer
		/// </summary>
		/// <param name="player"></param>
		public virtual void SecondaryAction(int timer, Player player)
		{

		}
	}

	public abstract class CombatMountItem : ModItem
	{
		protected CombatMount mount;

		public override string Texture => AssetDirectory.Debug;

		/// <summary>
		/// The Type of the CombatMount this item should summon. This must be a subclass of CombatMount.
		/// </summary>
		public abstract Type CombatMountType { get; }

		/// <summary>
		/// the numeric ID of the MountData supporting the CombatMount which this item summons.
		/// </summary>
		public abstract int MountType { get; }

		/// <summary>
		/// called after SetDefaults
		/// </summary>
		public virtual void SafeSetDefaults()
		{

		}

		public sealed override void SetDefaults()
		{
			mount = (CombatMount)Activator.CreateInstance(CombatMountType);
			mount.SetDefaults();

			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.damage = mount.damageCoefficient;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.useStyle = Terraria.ID.ItemUseStyleID.Swing;
			Item.mountType = MountType;

			SafeSetDefaults();
		}

		public override int ChoosePrefix(UnifiedRandom rand)
		{
			List<int> list = CombatMountPrefix.combatMountPrefixTypes;
			return list[rand.Next(list.Count())];
		}

		public override bool? UseItem(Player player) //Summon the mount on use
		{
			mount.ResetStats();
			mount.SetDefaults();

			ModPrefix prefix = PrefixLoader.GetPrefix(Item.prefix);

			if (prefix is CombatMountPrefix)
				(prefix as CombatMountPrefix).ApplyToMount(mount);

			mount.MountUp(player);

			player.GetModPlayer<CombatMountPlayer>().mountingTime = 30;
			player.GetModPlayer<CombatMountPlayer>().startPoint = player.Center;

			return true;
		}
	}
}
