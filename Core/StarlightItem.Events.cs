﻿using Terraria.DataStructures;

namespace StarlightRiver.Core
{
	internal partial class StarlightItem : GlobalItem
	{
		public delegate void ExtractinatorUseDelegate(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack);
		public static event ExtractinatorUseDelegate ExtractinatorUseEvent;
		public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
		{
			ExtractinatorUseEvent?.Invoke(extractType, extractinatorBlockType, ref resultType, ref resultStack);
		}

		public delegate void GetHealLifeDelegate(Item Item, Player Player, bool quickHeal, ref int healValue);
		public static event GetHealLifeDelegate GetHealLifeEvent;
		public override void GetHealLife(Item Item, Player Player, bool quickHeal, ref int healValue)
		{
			GetHealLifeEvent?.Invoke(Item, Player, quickHeal, ref healValue);
		}

		public delegate void GetHealManaDelegate(Item item, Player player, bool quickHeal, ref int healValue);
		public static event GetHealManaDelegate GetHealManaEvent;
		public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
		{
			base.GetHealMana(item, player, quickHeal, ref healValue);
		}

		public delegate void ModifyWeaponDamageDelegate(Item Item, Player Player, ref StatModifier statModifier);
		public static event ModifyWeaponDamageDelegate ModifyWeaponDamageEvent;
		public override void ModifyWeaponDamage(Item Item, Player Player, ref StatModifier statModifier)
		{
			ModifyWeaponDamageEvent?.Invoke(Item, Player, ref statModifier);
		}

		public delegate void GetWeaponCritDelegate(Item Item, Player Player, ref float crit);
		public static event GetWeaponCritDelegate GetWeaponCritEvent;
		public override void ModifyWeaponCrit(Item Item, Player Player, ref float crit)
		{
			GetWeaponCritEvent?.Invoke(Item, Player, ref crit);
		}

		public delegate void PickAmmoDelegate(Item weapon, Item ammo, Player Player, ref int type, ref float speed, ref StatModifier damage, ref float knockback);
		public static event PickAmmoDelegate PickAmmoEvent;
		public override void PickAmmo(Item weapon, Item ammo, Player Player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
		{
			PickAmmoEvent?.Invoke(weapon, ammo, Player, ref type, ref speed, ref damage, ref knockback);
		}

		public delegate bool ShootDelegate(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);
		public static event ShootDelegate ShootEvent;
		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (ShootEvent != null)
			{
				bool result = true;
				foreach (ShootDelegate del in ShootEvent.GetInvocationList())
				{
					result &= del(item, player, source, position, velocity, type, damage, knockback);
				}

				return result;
			}

			return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}

		public delegate bool OnPickupDelegate(Item Item, Player Player);
		public static event OnPickupDelegate OnPickupEvent;
		public override bool OnPickup(Item Item, Player Player)
		{
			if (OnPickupEvent != null)
			{
				bool result = true;
				foreach (OnPickupDelegate del in OnPickupEvent.GetInvocationList())
				{
					result &= del(Item, Player);
				}

				return result;
			}

			return base.OnPickup(Item, Player);
		}

		public delegate bool CanUseItemDelegate(Item Item, Player Player);
		public static event CanUseItemDelegate CanUseItemEvent;
		public override bool CanUseItem(Item Item, Player Player)
		{
			if (CanUseItemEvent != null)
			{
				bool result = true;
				foreach (CanUseItemDelegate del in CanUseItemEvent.GetInvocationList())
				{
					result &= del(Item, Player);
				}

				return result;
			}

			return base.CanUseItem(Item, Player);
		}

		public delegate bool? CanAutoReuseItemDelegate(Item Item, Player Player);
		public static event CanAutoReuseItemDelegate CanAutoReuseItemEvent;
		public override bool? CanAutoReuseItem(Item Item, Player Player)
		{
			if (CanAutoReuseItemEvent != null)
			{
				bool? result = true;
				foreach (CanAutoReuseItemDelegate del in CanAutoReuseItemEvent.GetInvocationList())
				{
					result &= del(Item, Player);
				}

				return result;
			}

			return base.CanAutoReuseItem(Item, Player);
		}

		public delegate bool AltFunctionUseDelegate(Item item, Player player);
		public static event AltFunctionUseDelegate AltFunctionUseEvent;
		public override bool AltFunctionUse(Item item, Player player)
		{
			if (AltFunctionUseEvent != null)
			{
				bool result = false;
				foreach (AltFunctionUseDelegate del in AltFunctionUseEvent.GetInvocationList())
				{
					result |= del(item, player);
				}

				return result;
			}

			return base.AltFunctionUse(item, player);
		}

		public delegate bool CanEquipAccessoryDelegate(Item item, Player player, int slot, bool modded);
		public static event CanEquipAccessoryDelegate CanEquipAccessoryEvent;
		public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
		{
			if (CanEquipAccessoryEvent != null)
			{
				bool result = true;
				foreach (CanEquipAccessoryDelegate del in CanEquipAccessoryEvent.GetInvocationList())
				{
					result &= del(item, player, slot, modded);
				}

				return result;
			}

			return true;
		}

		public delegate bool CanAccessoryBeEquippedWithDelegate(Item equippedItem, Item incomingItem, Player player);
		public static event CanAccessoryBeEquippedWithDelegate CanAccessoryBeEquippedWithEvent;
		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
		{
			if (CanAccessoryBeEquippedWithEvent != null)
			{
				bool result = true;
				foreach (CanAccessoryBeEquippedWithDelegate del in CanAccessoryBeEquippedWithEvent.GetInvocationList())
				{
					result &= del(equippedItem, incomingItem, player);
				}

				return result;
			}

			return true;
		}

		public delegate void ModifyItemLootDelegate(Item item, ItemLoot itemLoot);
		public static event ModifyItemLootDelegate ModifyItemLootEvent;
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			ModifyItemLootEvent?.Invoke(item, itemLoot);
		}

		public delegate bool? UseItemDelegate(Item Item, Player Player);
		public static event UseItemDelegate UseItemEvent;
		public override bool? UseItem(Item Item, Player Player)
		{
			if (UseItemEvent != null)
			{
				bool? result = true;
				foreach (UseItemDelegate del in UseItemEvent.GetInvocationList())
				{
					result &= del(Item, Player);
				}

				return result;
			}

			return base.UseItem(Item, Player);
		}

		public delegate float UseTimeMultiplierDelegate(Item item, Player player);
		public static event UseTimeMultiplierDelegate UseTimeMultiplierEvent;
		public override float UseTimeMultiplier(Item item, Player player)
		{
			float toReturn = 1;
			foreach (UseTimeMultiplierDelegate del in UseTimeMultiplierEvent.GetInvocationList())
			{
				toReturn *= del(item, player);
			}

			return toReturn;
		}

		public delegate float UseAnimationMultiplierDelegate(Item item, Player player);
		public static event UseAnimationMultiplierDelegate UseAnimationMultiplierEvent;
		public override float UseAnimationMultiplier(Item item, Player player)
		{
			float toReturn = 1;
			foreach (UseAnimationMultiplierDelegate del in UseAnimationMultiplierEvent.GetInvocationList())
			{
				toReturn *= del(item, player);
			}

			return toReturn;
		}

		public override void Unload()
		{
			ExtractinatorUseEvent = null;
			GetHealLifeEvent = null;
			ModifyWeaponDamageEvent = null;
			GetWeaponCritEvent = null;
			CanAutoReuseItemEvent = null;
			PickAmmoEvent = null;
			OnPickupEvent = null;
			CanUseItemEvent = null;
			CanEquipAccessoryEvent = null;
			CanAccessoryBeEquippedWithEvent = null;
			ModifyItemLootEvent = null;
			UseItemEvent = null;
			UseTimeMultiplierEvent = null;
			UseAnimationMultiplierEvent = null;
		}
	}
}