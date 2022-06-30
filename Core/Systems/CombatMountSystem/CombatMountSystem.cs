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
	internal class CombatMountSystem : ModPlayer
	{
		public CombatMount activeMount;

		public int mountingTime;

		public override void PreUpdateMovement()
		{
			if (Player.mount is null || !Player.mount.Active)
				Dismount();

			if (activeMount is null)
				return;

			if (activeMount.useCooldown > 0) activeMount.useCooldown --;
			if (activeMount.secondaryCooldown > 0) activeMount.secondaryCooldown--;

			if (activeMount.useTimer > 0)
			{
				activeMount.useTimer--;
				activeMount.PrimaryAction(activeMount.useTimer, Player);
			}

			if (activeMount.secondaryTimer > 0)
			{
				activeMount.secondaryTimer--;
				activeMount.SecondaryAction(activeMount.useTimer, Player);
			}
		}

		public void Dismount()
		{
			activeMount = null;
			Player.mount.Dismount(Player);
		}
	}

	internal class CombatMountOpacityChanger : GlobalItem
	{
		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			var isValid = ((item.DamageType.Type == DamageClass.Summon.Type || item.DamageType.Type == DamageClass.SummonMeleeSpeed.Type) && !Main.LocalPlayer.controlSmart);

			if (!Main.playerInventory && !isValid && Main.LocalPlayer.GetModPlayer<CombatMountSystem>().activeMount != null)
			{
				var tex = Terraria.GameContent.TextureAssets.Item[item.type].Value;
				spriteBatch.Draw(tex, position, frame, drawColor * 0.25f, 0, origin, scale, 0, 0);
				return false;
			}

			return true;
		}

		public override bool? CanAutoReuseItem(Item item, Player player)
		{
			var activeMount = player.GetModPlayer<CombatMountSystem>().activeMount;

			if (activeMount != null)
				return activeMount.autoReuse;

			return null;
		}

		public override bool AltFunctionUse(Item item, Player player)
		{
			var activeMount = player.GetModPlayer<CombatMountSystem>().activeMount;

			if (activeMount != null)
				return true;

			return false;
		}

		public override bool CanUseItem(Item item, Player player)
		{
			if (item == player.HeldItem)
			{
				var activeMount = player.GetModPlayer<CombatMountSystem>().activeMount;

				if (activeMount != null && ((item.DamageType.Type != DamageClass.Summon.Type && item.DamageType.Type != DamageClass.SummonMeleeSpeed.Type) || player.controlSmart))
				{
					if (Main.mouseRight && activeMount.secondaryTimer == 0 && activeMount.secondaryCooldown <= 0)
						activeMount.StartSecondaryAction(player);

					if (Main.mouseLeft && activeMount.useTimer == 0 && activeMount.useCooldown <= 0)
						activeMount.StartPrimaryAction(player);

					return false;
				}

				if (item.ModItem is null || (item.ModItem != null && !item.ModItem.AltFunctionUse(player)))
				{
					if (Main.mouseRight && activeMount.secondaryTimer == 0 && activeMount.secondaryCooldown <= 0)
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
