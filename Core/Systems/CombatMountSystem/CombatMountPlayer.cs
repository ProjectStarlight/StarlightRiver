using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Systems.CombatMountSystem
{
	internal class CombatMountPlayer : ModPlayer
	{
		public CombatMount activeMount;

		public int mountingTime;

		public override void PreUpdateMovement()
		{
			if (Player.mount is null || !Player.mount.Active)
				Dismount();

			if (activeMount is null)
				return;

			if (activeMount.primaryCooldownTimer > 0) activeMount.primaryCooldownTimer --;
			if (activeMount.secondaryCooldownTimer > 0) activeMount.secondaryCooldownTimer--;

			if (activeMount.primaryAttackTimer > 0)
			{
				activeMount.primaryAttackTimer--;
				activeMount.PrimaryAction(activeMount.primaryAttackTimer, Player);
			}

			if (activeMount.secondaryAbilityTimer > 0)
			{
				activeMount.secondaryAbilityTimer--;
				activeMount.SecondaryAction(activeMount.primaryAttackTimer, Player);
			}

			activeMount.UpdatePhysics(Player);
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
			var isValid = ((item.DamageType.Type == DamageClass.Summon.Type || item.DamageType.Type == DamageClass.SummonMeleeSpeed.Type) && !Main.LocalPlayer.controlSmart);

			if (!Main.playerInventory && !isValid && Main.LocalPlayer.GetModPlayer<CombatMountPlayer>().activeMount != null)
			{
				var tex = Terraria.GameContent.TextureAssets.Item[item.type].Value;
				spriteBatch.Draw(tex, position, frame, drawColor * 0.25f, 0, origin, scale, 0, 0);
				return false;
			}

			return true;
		}

		public override bool? CanAutoReuseItem(Item item, Player player)
		{
			var activeMount = player.GetModPlayer<CombatMountPlayer>().activeMount;

			if (activeMount != null)
				return activeMount.autoReuse;

			return null;
		}

		public override bool AltFunctionUse(Item item, Player player)
		{
			var activeMount = player.GetModPlayer<CombatMountPlayer>().activeMount;

			if (activeMount != null)
				return true;

			return false;
		}

		public override bool CanUseItem(Item item, Player player)
		{
			if (item == player.HeldItem)
			{
				var activeMount = player.GetModPlayer<CombatMountPlayer>().activeMount;

				if (activeMount != null && ((item.DamageType.Type != DamageClass.Summon.Type && item.DamageType.Type != DamageClass.SummonMeleeSpeed.Type) || player.controlSmart))
				{
					if (Main.mouseRight && activeMount.secondaryAbilityTimer == 0 && activeMount.secondaryCooldownTimer <= 0)
						activeMount.StartSecondaryAction(player);

					if (Main.mouseLeft && activeMount.primaryAttackTimer == 0 && activeMount.primaryCooldownTimer <= 0)
						activeMount.StartPrimaryAction(player);

					return false;
				}

				if (item.ModItem is null || (item.ModItem != null && !item.ModItem.AltFunctionUse(player)))
				{
					if (Main.mouseRight && activeMount.secondaryAbilityTimer == 0 && activeMount.secondaryCooldownTimer <= 0)
					{
						activeMount.StartSecondaryAction(player);
						return false;
					}
				}
			}

			return true;
		}
	}
}
