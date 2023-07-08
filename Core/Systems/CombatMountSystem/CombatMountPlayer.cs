﻿using System;

namespace StarlightRiver.Core.Systems.CombatMountSystem
{
	internal class CombatMountPlayer : ModPlayer
	{
		public CombatMount activeMount;

		public int mountingTime;
		public Vector2 startPoint;

		public override void Load()
		{
			On_Player.ItemCheck_Inner += TriggerMountAttacks;
		}

		private void TriggerMountAttacks(On_Player.orig_ItemCheck_Inner orig, Player self)
		{
			CombatMount activeMount = self.GetModPlayer<CombatMountPlayer>().activeMount;
			Item sItem = self.HeldItem;

			if (activeMount is null || self.CCed || !self.controlUseItem || !self.releaseUseItem || self.itemAnimation != 0)
			{
				orig(self);
				return;
			}

			if (sItem.DamageType.Type != DamageClass.Summon.Type && sItem.DamageType.Type != DamageClass.SummonMeleeSpeed.Type || self.controlSmart)
			{
				self.releaseUseItem = activeMount.autoReuse;
				self.controlUseItem = false;

				if (Main.mouseRight && activeMount.secondaryAbilityTimer == 0 && activeMount.secondaryCooldownTimer <= 0)
					activeMount.StartSecondaryAction(self);

				if (Main.mouseLeft && activeMount.primaryAttackTimer == 0 && activeMount.primaryCooldownTimer <= 0)
					activeMount.StartPrimaryAction(self);

				return;
			}

			if (sItem.ModItem is null || sItem.ModItem != null && !sItem.ModItem.AltFunctionUse(self))
			{
				if (Main.mouseRight && activeMount.secondaryAbilityTimer == 0 && activeMount.secondaryCooldownTimer <= 0)
				{
					self.releaseUseItem = activeMount.autoReuse;
					self.controlUseItem = false;

					activeMount.StartSecondaryAction(self);
					return;
				}
			}

			orig(self);
		}

		public override void PreUpdateMovement() //Updates the active mount's timers and calls their actions.
		{
			if (Player.mount is null || !Player.mount.Active)
				Dismount();

			if (activeMount is null)
				return;

			if (mountingTime > 0)
				mountingTime--;

			if (Player.HeldItem.IsAir && Main.mouseRight && !Player.releaseUseItem && activeMount.secondaryAbilityTimer == 0 && activeMount.secondaryCooldownTimer <= 0) //special case for shooting while holding air
				activeMount.StartSecondaryAction(Player);

			if (activeMount.primaryCooldownTimer > 0)
				activeMount.primaryCooldownTimer--;

			if (activeMount.secondaryCooldownTimer > 0)
				activeMount.secondaryCooldownTimer--;

			if (activeMount.primaryAttackTimer > 0)
			{
				activeMount.primaryAttackTimer--;
				activeMount.PrimaryAction(activeMount.primaryAttackTimer, Player);
			}

			if (activeMount.secondaryAbilityTimer > 0)
			{
				activeMount.secondaryAbilityTimer--;
				activeMount.SecondaryAction(activeMount.secondaryAbilityTimer, Player);
			}

			activeMount.PostUpdate(Player);
		}

		public override void PostUpdate()
		{
			if (mountingTime > 0)
				Player.gfxOffY = Helpers.Helper.LerpFloat(Player.mount.PlayerOffset, 0, 1 - mountingTime / 30f) - (float)Math.Sin(3.14f * (1 - mountingTime / 30f)) * 64;
		}

		public void Dismount()
		{
			activeMount = null;
			Player.mount.Dismount(Player);
		}
	}

	internal class CombatMountGlobalItem : GlobalItem
	{
		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (Main.gameMenu)
				return true;

			bool isValid = (item.DamageType.Type == DamageClass.Summon.Type || item.DamageType.Type == DamageClass.SummonMeleeSpeed.Type) && !Main.LocalPlayer.controlSmart;

			if (!Main.playerInventory && !isValid && Main.LocalPlayer.GetModPlayer<CombatMountPlayer>().activeMount != null)
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.Item[item.type].Value;
				spriteBatch.Draw(tex, position, frame, drawColor * 0.25f, 0, origin, scale, 0, 0);
				return false;
			}

			return true;
		}

		public override bool AltFunctionUse(Item item, Player player)
		{
			CombatMount activeMount = player.GetModPlayer<CombatMountPlayer>().activeMount;

			if (activeMount != null)
				return true;

			return false;
		}

		public override bool CanUseItem(Item item, Player player)
		{
			CombatMount activeMount = player.GetModPlayer<CombatMountPlayer>().activeMount;

			if (activeMount != null)
			{
				if (item.DamageType.Type != DamageClass.Summon.Type && item.DamageType.Type != DamageClass.SummonMeleeSpeed.Type || player.controlSmart)
					return false;

				if (item.ModItem is null || item.ModItem != null && !item.ModItem.AltFunctionUse(player))
				{
					if (Main.mouseRight && activeMount.secondaryAbilityTimer == 0 && activeMount.secondaryCooldownTimer <= 0)
						return false;
				}
			}

			return true;
		}
	}
}