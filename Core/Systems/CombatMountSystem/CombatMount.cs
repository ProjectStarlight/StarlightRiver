using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.Mount;

namespace StarlightRiver.Core.Systems.CombatMountSystem
{
	public abstract class CombatMount
	{
		public int useTimer;
		public int secondaryTimer;

		public int useCooldown;
		public int secondaryCooldown;

		public float actionSpeedMultiplier = 1;
		public float secondaryCooldownSpeedMultiplier = 1;
		public float moveSpeedMultiplier = 1;
		public int damageCoefficient;

		public int actionSpeedCoefficient;
		public int secondarySpeedCoefficient;

		public int actionCooldownCoefficient;
		public int secondaryCooldownCoefficient;

		public int MaxActionTime => (int)(actionSpeedCoefficient * actionSpeedMultiplier);
		public int MaxActionCD => MaxActionTime + actionCooldownCoefficient;
		public int MaxSecondaryCD => (int)(secondaryCooldownCoefficient * secondaryCooldownSpeedMultiplier + secondarySpeedCoefficient);

		public virtual string PrimaryIconTexture => AssetDirectory.Debug;
		public virtual string SecondaryIconTexture => AssetDirectory.Debug;

		public void MountUp(Player player)
		{
			SetDefaults();
			player.GetModPlayer<CombatMountSystem>().activeMount = this;
		}

		public virtual void SetDefaults()
		{

		}

		public void StartPrimaryAction(Player player)
		{
			useTimer = MaxActionTime;
			useCooldown = actionCooldownCoefficient + useTimer;
			OnStartPrimaryAction(player);
		}

		public virtual void OnStartPrimaryAction(Player player)
		{

		}

		public virtual void PrimaryAction(int timer, Player player)
		{

		}

		public void StartSecondaryAction(Player player)
		{
			secondaryTimer = secondarySpeedCoefficient;
			secondaryCooldown = (int)(secondaryCooldownCoefficient * secondaryCooldownSpeedMultiplier) + secondaryTimer;
			OnStartSecondaryAction(player);
		}

		public virtual void OnStartSecondaryAction(Player player)
		{

		}

		public virtual void SecondaryAction(int timer, Player player)
		{

		}
	}

	internal class CombatMountUnderlyingMountdata : ModMount
	{

	}
}
