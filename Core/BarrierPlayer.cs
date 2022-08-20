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
	public class BarrierPlayer : ModPlayer //yay we have to duplicate a ton of code because terraria has no base entity class that Players and NPCs share
	{
		public bool JustHitWithBarrier = false;

		public int MaxBarrier = 0;
		public int Barrier = 0;
		public bool PlayerCanLiveWithOnlyBarrier = false;

		public bool DontDrainOvercharge = false;
		public int OverchargeDrainRate = 60;

		public int TimeSinceLastHit = 0;
		public int RechargeDelay = 480;
		public int RechargeRate = 4;

		public float BarrierDamageReduction = 0.3f;

		public float RechargeAnimationTimer;
		public Item barrierDyeItem;

		public bool sendUpdatePacket = true; // set this to true whenever something else happens that would desync shield values, for example: onhit effects

		public BarrierDye dye
		{
			get
			{
				if (barrierDyeItem is null || barrierDyeItem.IsAir)
                {
					Item Item = new Item();
					Item.SetDefaults(ModContent.ItemType<BaseBarrierDye>());
					barrierDyeItem = Item;
				}

				return (barrierDyeItem.ModItem as BarrierDye);
			}
		}
		public override void Load()
		{
			StarlightPlayer.PostDrawEvent += PostDrawBarrierFX;
			StarlightPlayer.PreDrawEvent += PreDrawBarrierFX;

			
		}

		private void PostDrawBarrierFX(Player Player, SpriteBatch spriteBatch)
		{
			if(!Main.gameMenu)
				Player.GetModPlayer<BarrierPlayer>().dye?.PostDrawEffects(spriteBatch, Player);
		}

		private void PreDrawBarrierFX(Player Player, SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu)
				Player.GetModPlayer<BarrierPlayer>().dye?.PreDrawEffects(spriteBatch, Player);
		}

		public void ModifyDamage(ref int damage, ref bool crit)
		{
			if (Barrier > 0)
			{
				float reduction = 1.0f - BarrierDamageReduction;

				if (Barrier > damage)
				{
					dye?.HitBarrierEffects(Player);

					CombatText.NewText(Player.Hitbox, Color.Cyan, damage);
					Barrier -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					RechargeAnimationTimer = 0;

					dye?.LoseBarrierEffects(Player);

					CombatText.NewText(Player.Hitbox, Color.Cyan, Barrier);
					int overblow = damage - Barrier;
					damage = (int)(Barrier * reduction) + overblow;

					Barrier = 0;
				}
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (Barrier > 0)
			{
				damage = (int)Main.CalculateDamagePlayersTake(damage, Player.statDefense);

				ModifyDamage(ref damage, ref crit);
				TimeSinceLastHit = 0;
				Player.statDefense = 0;
				JustHitWithBarrier = true;
			}

			return true;
		}

		public override void UpdateBadLifeRegen()
		{
			if (Barrier > 0)
			{
				Helper.UnlockCodexEntry<BarrierEntry>(Main.LocalPlayer);

				if (RechargeAnimationTimer < 1)
				{
					if (dye != null)
						RechargeAnimationTimer += dye.RechargeAnimationRate;
					else
						RechargeAnimationTimer += 0.05f;
				}
			}
			else 
				RechargeAnimationTimer = 0;

			if (MaxBarrier > 0)
				TimeSinceLastHit++;

			if (MaxBarrier == 0)
				TimeSinceLastHit = 0;

			if (TimeSinceLastHit >= RechargeDelay)
			{
				if (Barrier < MaxBarrier)
				{
					int rechargeRateWhole = RechargeRate / 60;

					Barrier += Math.Min(rechargeRateWhole, MaxBarrier - Barrier);

					if (RechargeRate % 60 != 0)
					{
						int rechargeSubDelay = 60 / (RechargeRate % 60);

						if (TimeSinceLastHit % rechargeSubDelay == 0 && Barrier < MaxBarrier)
							Barrier++;
					}
				}
			}

			OverchargeDrainRate = Math.Max(0, OverchargeDrainRate);

			if (Barrier > MaxBarrier && !DontDrainOvercharge)
			{
				int drainRateWhole = OverchargeDrainRate / 60;

				Barrier -= Math.Min(drainRateWhole, Barrier - MaxBarrier);

				if (OverchargeDrainRate % 60 != 0)
				{
					int drainSubDelay = 60 / (OverchargeDrainRate % 60);

					if (Player.GetModPlayer<StarlightPlayer>().Timer % drainSubDelay == 0 && Barrier > MaxBarrier)
						Barrier--;
				}
			}
		}

		public override void PostUpdate() //change inventory screen for dyes
		{
			if (Main.mouseItem.ModItem is BarrierDye)
				Main.EquipPageSelected = 2;

			JustHitWithBarrier = false;
		}

		public override void UpdateDead()
		{
			Barrier = 0;
			TimeSinceLastHit = 0;
		}

		public override void clientClone(ModPlayer clientClone)
		{
			BarrierPlayer clone = clientClone as BarrierPlayer;
			// Here we would make a backup clone of values that are only correct on the local Players Player instance.
			clone.barrierDyeItem = barrierDyeItem;
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
        {
			BarrierPlayer clone = clientPlayer as BarrierPlayer;
			if (sendUpdatePacket || clone?.barrierDyeItem?.type != barrierDyeItem?.type)
            {
				ShieldPacket packet = new ShieldPacket(this);
				packet.Send(-1, Player.whoAmI, false);
			}
		}
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (PlayerCanLiveWithOnlyBarrier && Barrier > 0) //if the Player has no max life, its implied they can live off of shield
				return false;

			return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["DyeItem"] = barrierDyeItem;
		}

		public override void LoadData(TagCompound tag)
		{
			barrierDyeItem = tag.Get<Item>("DyeItem");
		}

		public override void ResetEffects()
		{
			MaxBarrier = 0;
			PlayerCanLiveWithOnlyBarrier = false;

			DontDrainOvercharge = false;
			OverchargeDrainRate = 60;

			RechargeDelay = 480;
			RechargeRate = 4;

			BarrierDamageReduction = Main.expertMode ? 0.4f : 0.3f;

			if (dye is null)
			{
				Item Item = new Item();
				Item.SetDefaults(ModContent.ItemType<BaseBarrierDye>());
				barrierDyeItem = Item;
			}
		}
	}

	[Serializable]
	public class ShieldPacket : Module
	{
		public readonly byte whoAmI;
		public readonly int shield;
		public readonly int dyeType;

		public ShieldPacket(BarrierPlayer sPlayer)
		{
			whoAmI = (byte)sPlayer.Player.whoAmI;
			shield = sPlayer.Barrier;

			if (sPlayer.barrierDyeItem is null)
				dyeType = ModContent.ItemType<BaseBarrierDye>();
			else
				dyeType = sPlayer.barrierDyeItem.type;
		}

		protected override void Receive()
		{
			BarrierPlayer Player = Main.player[whoAmI].GetModPlayer<BarrierPlayer>();

			Player.Barrier = shield;
			
			if (Player.barrierDyeItem is null || Player.barrierDyeItem.type != dyeType)
            {
				Item Item = new Item();
				Item.SetDefaults(dyeType);
				Player.barrierDyeItem = Item;
				Player.RechargeAnimationTimer = 0;
            }

			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				Send(-1, Player.Player.whoAmI, false);
				return;
			}
		}
	}

}
