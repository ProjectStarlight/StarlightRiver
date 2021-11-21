using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetEasy;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Content.Items.BarrierDye;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

namespace StarlightRiver.Core
{
	public class ShieldPlayer : ModPlayer //yay we have to duplicate a ton of code because terraria has no base entity class that players and NPCs share
	{
		public int MaxShield = 0;
		public int Shield = 0;
		public bool LiveOnOnlyShield = false;

		public bool DontDrainOvershield = false;
		public int OvershieldDrainRate = 60;

		public int TimeSinceLastHit = 0;
		public int RechargeDelay = 480;
		public int RechargeRate = 4;

		public float ShieldResistance = 0.3f;

		public float rechargeAnimation;
		public Item barrierDyeItem;

		public bool sendUpdatePacket = true; // set this to true whenever something else happens that would desync shield values, for example: onhit effects

		public BarrierDye dye
		{
			get
			{
				if (barrierDyeItem is null || barrierDyeItem.IsAir)
                {
					Item item = new Item();
					item.SetDefaults(ModContent.ItemType<BaseBarrierDye>());
					barrierDyeItem = item;
				}

				return (barrierDyeItem.modItem as BarrierDye);
			}
		}
		public override bool Autoload(ref string name)
		{
			StarlightPlayer.PostDrawEvent += PostDrawBarrierFX;
			StarlightPlayer.PreDrawEvent += PreDrawBarrierFX;

			return base.Autoload(ref name);
		}

		private void PostDrawBarrierFX(Player player, SpriteBatch spriteBatch)
		{
			if(!Main.gameMenu)
				player.GetModPlayer<ShieldPlayer>().dye?.PostDrawEffects(spriteBatch, player);
		}

		private void PreDrawBarrierFX(Player player, SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu)
				player.GetModPlayer<ShieldPlayer>().dye?.PreDrawEffects(spriteBatch, player);
		}

		public void ModifyDamage(ref int damage, ref bool crit)
		{
			if (Shield > 0)
			{
				float reduction = 1.0f - ShieldResistance;

				if (Shield > damage)
				{
					dye?.HitBarrierEffects(player);

					CombatText.NewText(player.Hitbox, Color.Cyan, damage);
					Shield -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					rechargeAnimation = 0;

					dye?.LoseBarrierEffects(player);

					CombatText.NewText(player.Hitbox, Color.Cyan, Shield);
					int overblow = damage - Shield;
					damage = (int)(Shield * reduction) + overblow;

					Shield = 0;
				}
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (Shield > 0)
			{
				damage = (int)Main.CalculatePlayerDamage(damage, player.statDefense);

				ModifyDamage(ref damage, ref crit);
				TimeSinceLastHit = 0;
				player.statDefense = 0;
			}

			return true;
		}

		public override void UpdateBadLifeRegen()
		{
			if (Shield > 0)
			{
				Helper.UnlockEntry<BarrierEntry>(Main.LocalPlayer);

				if (rechargeAnimation < 1)
				{
					if (dye != null)
						rechargeAnimation += dye.RechargeAnimationRate;
					else
						rechargeAnimation += 0.05f;
				}
			}
			else 
				rechargeAnimation = 0;

			if (MaxShield > 0)
				TimeSinceLastHit++;

			if (MaxShield == 0)
				TimeSinceLastHit = 0;

			if (TimeSinceLastHit >= RechargeDelay)
			{
				if (Shield < MaxShield)
				{
					int rechargeRateWhole = RechargeRate / 60;

					Shield += Math.Min(rechargeRateWhole, MaxShield - Shield);

					if (RechargeRate % 60 != 0)
					{
						int rechargeSubDelay = 60 / (RechargeRate % 60);

						if (TimeSinceLastHit % rechargeSubDelay == 0 && Shield < MaxShield)
							Shield++;
					}
				}
			}

			OvershieldDrainRate = Math.Max(0, OvershieldDrainRate);

			if (Shield > MaxShield && !DontDrainOvershield)
			{
				int drainRateWhole = OvershieldDrainRate / 60;

				Shield -= Math.Min(drainRateWhole, Shield - MaxShield);

				if (OvershieldDrainRate % 60 != 0)
				{
					int drainSubDelay = 60 / (OvershieldDrainRate % 60);

					if (player.GetModPlayer<StarlightPlayer>().Timer % drainSubDelay == 0 && Shield > MaxShield)
						Shield--;
				}
			}
		}

		public override void PostUpdate() //change inventory screen for dyes
		{
			if (Main.mouseItem.modItem is BarrierDye)
				Main.EquipPageSelected = 2;
		}

		public override void UpdateDead()
		{
			Shield = 0;
			TimeSinceLastHit = 0;
		}

		public override void clientClone(ModPlayer clientClone)
		{
			ShieldPlayer clone = clientClone as ShieldPlayer;
			// Here we would make a backup clone of values that are only correct on the local players Player instance.
			clone.barrierDyeItem = barrierDyeItem;
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
        {
			ShieldPlayer clone = clientPlayer as ShieldPlayer;
			if (sendUpdatePacket || clone?.barrierDyeItem?.type != barrierDyeItem?.type)
            {
				ShieldPacket packet = new ShieldPacket(this);
				packet.Send(-1, player.whoAmI, false);
			}
		}
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (LiveOnOnlyShield && Shield > 0) //if the player has no max life, its implied they can live off of shield
				return false;

			return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
		}

		public override TagCompound Save()
		{
			return new TagCompound()
			{
				["DyeItem"] = barrierDyeItem
			};
		}

		public override void Load(TagCompound tag)
		{
			barrierDyeItem = tag.Get<Item>("DyeItem");
		}

		public override void ResetEffects()
		{
			MaxShield = 0;
			LiveOnOnlyShield = false;

			DontDrainOvershield = false;
			OvershieldDrainRate = 60;

			RechargeDelay = 480;
			RechargeRate = 4;

			ShieldResistance = Main.expertMode ? 0.4f : 0.3f;

			if (dye is null)
			{
				Item item = new Item();
				item.SetDefaults(ModContent.ItemType<BaseBarrierDye>());
				barrierDyeItem = item;
			}
		}
	}

	[Serializable]
	public class ShieldPacket : Module
	{
		public readonly sbyte whoAmI;
		public readonly int shield;
		public readonly int dyeType;

		public ShieldPacket(ShieldPlayer sPlayer)
		{
			whoAmI = (sbyte)sPlayer.player.whoAmI;
			shield = sPlayer.Shield;

			if (sPlayer.barrierDyeItem is null)
				dyeType = ModContent.ItemType<BaseBarrierDye>();
			else
				dyeType = sPlayer.barrierDyeItem.type;
		}

		protected override void Receive()
		{
			ShieldPlayer player = Main.player[whoAmI].GetModPlayer<ShieldPlayer>();

			player.Shield = shield;
			
			if (player.barrierDyeItem is null || player.barrierDyeItem.type != dyeType)
            {
				Item item = new Item();
				item.SetDefaults(dyeType);
				player.barrierDyeItem = item;
				player.rechargeAnimation = 0;
            }

			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				Send(-1, player.player.whoAmI, false);
				return;
			}
		}
	}

}
