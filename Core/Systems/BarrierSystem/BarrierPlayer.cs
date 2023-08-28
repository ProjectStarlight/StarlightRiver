using NetEasy;
using StarlightRiver.Content.Items.BarrierDye;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.BarrierSystem
{
	public class BarrierPlayer : ModPlayer //yay we have to duplicate a ton of code because terraria has no base entity class that Players and NPCs share
	{
		public bool justHitWithBarrier = false;

		public int maxBarrier = 0;
		public int barrier = 0;
		public bool playerCanLiveWithOnlyBarrier = false;

		public bool dontDrainOvercharge = false;
		public int overchargeDrainRate = 60;

		public int timeSinceLastHit = 1;
		public int rechargeDelay = 300;
		public int rechargeRate = 6;

		public float barrierDamageReduction = 0.5f;

		public float rechargeAnimationTimer;
		public Item barrierDyeItem;

		public bool sendUpdatePacket = false; // set this to true whenever something else happens that would desync shield values, for example: onhit effects

		public BarrierDye Dye
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
				Player.GetModPlayer<BarrierPlayer>().Dye?.PostDrawEffects(spriteBatch, Player);
		}

		private void PreDrawBarrierFX(Player Player, SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu)
				Player.GetModPlayer<BarrierPlayer>().Dye?.PreDrawEffects(spriteBatch, Player);
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (barrier > 0)
			{
				//We need to use the backdoor here since we need to know the final damage to subtract from barrier
				modifiers.ModifyHurtInfo += ModifyDamage;

				timeSinceLastHit = 0;
				justHitWithBarrier = true;
			}
		}

		public void ModifyDamage(ref Player.HurtInfo info)
		{
			if (barrier > 0)
			{
				sendUpdatePacket = true; //possible TODO of reworking this into a playerhitpacket like the npc hit packet

				float reduction = 1.0f - barrierDamageReduction;

				if (barrier > info.Damage)
				{
					Dye?.HitBarrierEffects(Player);

					CombatText.NewText(Player.Hitbox, Color.Cyan, info.Damage);
					barrier -= info.Damage;
					info.Damage = (int)(info.Damage * reduction);
				}
				else
				{
					rechargeAnimationTimer = 0;

					Dye?.LoseBarrierEffects(Player);

					CombatText.NewText(Player.Hitbox, Color.Cyan, barrier);
					int overblow = info.Damage - barrier;
					info.Damage = (int)(barrier * reduction) + overblow;

					barrier = 0;
				}
			}
		}

		public override void UpdateBadLifeRegen()
		{
			if (barrier > 0)
			{
				if (rechargeAnimationTimer < 1)
				{
					if (Dye != null)
						rechargeAnimationTimer += Dye.RechargeAnimationRate;
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

			justHitWithBarrier = false;
		}

		public override void UpdateDead()
		{
			barrier = 0;
			timeSinceLastHit = 0;
		}

		public override void CopyClientState(ModPlayer clientClone)
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
				sendUpdatePacket = false;
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

			rechargeDelay = 300;
			rechargeRate = 6;

			barrierDamageReduction = 0.5f;

			if (Dye is null)
			{
				var Item = new Item();
				Item.SetDefaults(ModContent.ItemType<BaseBarrierDye>());
				barrierDyeItem = Item;
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			//for syncing on world joins
			var packet = new ShieldPacket(this);
			packet.Send(toWho, Player.whoAmI, false);
		}
	}

	[Serializable]
	public class ShieldPacket : Module
	{
		public readonly byte whoAmI;
		public readonly int shield;
		public readonly int dyeType;
		public readonly int timeSinceLastHit;

		public ShieldPacket(BarrierPlayer sPlayer)
		{
			whoAmI = (byte)sPlayer.Player.whoAmI;
			shield = sPlayer.barrier;

			if (sPlayer.barrierDyeItem is null)
				dyeType = ModContent.ItemType<BaseBarrierDye>();
			else
				dyeType = sPlayer.barrierDyeItem.type;

			timeSinceLastHit = sPlayer.timeSinceLastHit;
		}

		protected override void Receive()
		{
			BarrierPlayer Player = Main.player[whoAmI].GetModPlayer<BarrierPlayer>();

			int priorBarrier = Player.barrier;
			Player.barrier = shield;
			if (Player.barrier <= 0)
				Player.rechargeAnimationTimer = 0;

			Player.timeSinceLastHit = timeSinceLastHit;
			if (Player.timeSinceLastHit == 0 && Player.maxBarrier > 0) // probably close enough
				Player.justHitWithBarrier = true;

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