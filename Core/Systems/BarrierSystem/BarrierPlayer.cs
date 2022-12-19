using NetEasy;
using StarlightRiver.Content.Codex.Entries;
using StarlightRiver.Content.Items.BarrierDye;
using StarlightRiver.Helpers;
using System;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.BarrierSystem
{
	public class BarrierPlayer : ModPlayer //yay we have to duplicate a ton of code because terraria has no base entity class that Players and NPCs share
	{
		public int maxBarrier = 0;
		public int barrier = 0;
		public bool playerCanLiveWithOnlyBarrier = false;

		public bool dontDrainOvercharge = false;
		public int overchargeDrainRate = 60;

		public int timeSinceLastHit = 0;
		public int rechargeDelay = 480;
		public int rechargeRate = 4;

		public float barrierDamageReduction = 0.3f;

		public float rechargeAnimationTimer;
		public Item barrierDyeItem;

		public bool sendUpdatePacket = true; // set this to true whenever something else happens that would desync shield values, for example: onhit effects

		public BarrierDye dye
		{
			get
			{
				if (barrierDyeItem is null || barrierDyeItem.IsAir)
				{
					var Item = new Item();
					Item.SetDefaults(ModContent.ItemType<BaseBarrierDye>());
					barrierDyeItem = Item;
				}

				return barrierDyeItem.ModItem as BarrierDye;
			}
		}

		public override void Load()
		{
			StarlightPlayer.PostDrawEvent += PostDrawBarrierFX;
			StarlightPlayer.PreDrawEvent += PreDrawBarrierFX;

		}

		private void PostDrawBarrierFX(Player Player, SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu)
				Player.GetModPlayer<BarrierPlayer>().dye?.PostDrawEffects(spriteBatch, Player);
		}

		private void PreDrawBarrierFX(Player Player, SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu)
				Player.GetModPlayer<BarrierPlayer>().dye?.PreDrawEffects(spriteBatch, Player);
		}

		public void ModifyDamage(ref int damage, ref bool crit)
		{
			if (barrier > 0)
			{
				float reduction = 1.0f - barrierDamageReduction;

				if (barrier > damage)
				{
					dye?.HitBarrierEffects(Player);

					CombatText.NewText(Player.Hitbox, Color.Cyan, damage);
					barrier -= damage;
					damage = (int)(damage * reduction);
				}
				else
				{
					rechargeAnimationTimer = 0;

					dye?.LoseBarrierEffects(Player);

					CombatText.NewText(Player.Hitbox, Color.Cyan, barrier);
					int overblow = damage - barrier;
					damage = (int)(barrier * reduction) + overblow;

					barrier = 0;
				}
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
		{
			if (barrier > 0)
			{
				damage = (int)Main.CalculateDamagePlayersTake(damage, Player.statDefense);

				ModifyDamage(ref damage, ref crit);
				timeSinceLastHit = 0;
				Player.statDefense = 0;
			}

			return true;
		}

		public override void UpdateBadLifeRegen()
		{
			if (barrier > 0)
			{
				Helper.UnlockCodexEntry<BarrierEntry>(Main.LocalPlayer);

				if (rechargeAnimationTimer < 1)
				{
					if (dye != null)
						rechargeAnimationTimer += dye.RechargeAnimationRate;
					else
						rechargeAnimationTimer += 0.05f;
				}
			}
			else
			{
				rechargeAnimationTimer = 0;
			}

			if (maxBarrier > 0)
				timeSinceLastHit++;

			if (maxBarrier == 0)
				timeSinceLastHit = 0;

			if (timeSinceLastHit >= rechargeDelay)
			{
				if (barrier < maxBarrier)
				{
					int rechargeRateWhole = rechargeRate / 60;

					barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

					if (rechargeRate % 60 != 0)
					{
						int rechargeSubDelay = 60 / (rechargeRate % 60);

						if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
							barrier++;
					}
				}
			}

			overchargeDrainRate = Math.Max(0, overchargeDrainRate);

			if (barrier > maxBarrier && !dontDrainOvercharge)
			{
				int drainRateWhole = overchargeDrainRate / 60;

				barrier -= Math.Min(drainRateWhole, barrier - maxBarrier);

				if (overchargeDrainRate % 60 != 0)
				{
					int drainSubDelay = 60 / (overchargeDrainRate % 60);

					if (Player.GetModPlayer<StarlightPlayer>().Timer % drainSubDelay == 0 && barrier > maxBarrier)
						barrier--;
				}
			}
		}

		public override void PostUpdate() //change inventory screen for dyes
		{
			if (Main.mouseItem.ModItem is BarrierDye)
				Main.EquipPageSelected = 2;
		}

		public override void UpdateDead()
		{
			barrier = 0;
			timeSinceLastHit = 0;
		}

		public override void clientClone(ModPlayer clientClone)
		{
			var clone = clientClone as BarrierPlayer;
			// Here we would make a backup clone of values that are only correct on the local Players Player instance.
			clone.barrierDyeItem = barrierDyeItem;
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			var clone = clientPlayer as BarrierPlayer;

			if (sendUpdatePacket || clone?.barrierDyeItem?.type != barrierDyeItem?.type)
			{
				var packet = new ShieldPacket(this);
				packet.Send(-1, Player.whoAmI, false);
			}
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (playerCanLiveWithOnlyBarrier && barrier > 0) //if the Player has no max life, its implied they can live off of shield
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
			maxBarrier = 0;
			playerCanLiveWithOnlyBarrier = false;

			dontDrainOvercharge = false;
			overchargeDrainRate = 60;

			rechargeDelay = 480;
			rechargeRate = 4;

			barrierDamageReduction = Main.expertMode ? 0.4f : 0.3f;

			if (dye is null)
			{
				var Item = new Item();
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
			shield = sPlayer.barrier;

			if (sPlayer.barrierDyeItem is null)
				dyeType = ModContent.ItemType<BaseBarrierDye>();
			else
				dyeType = sPlayer.barrierDyeItem.type;
		}

		protected override void Receive()
		{
			BarrierPlayer Player = Main.player[whoAmI].GetModPlayer<BarrierPlayer>();

			Player.barrier = shield;

			if (Player.barrierDyeItem is null || Player.barrierDyeItem.type != dyeType)
			{
				var Item = new Item();
				Item.SetDefaults(dyeType);
				Player.barrierDyeItem = Item;
				Player.rechargeAnimationTimer = 0;
			}

			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				Send(-1, Player.Player.whoAmI, false);
				return;
			}
		}
	}
}
