using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.NPCs.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Hell
{
	internal class FuryInABottle : SmartAccessory
	{
		public int charge;

		public override string Texture => AssetDirectory.HellItem + Name;

		public FuryInABottle() : base("Fury in a Bottle", "Allows a short double jump\nRecharges for every 100 damage dealt") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += GainCharge;
			StarlightPlayer.OnHitNPCWithProjEvent += GainChargeProj;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 1, 0, 0);
		}

		private void GainCharge(Player player, Item Item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player) && !player.GetJumpState<FuryJump>().Available)
			{
				var instance = GetEquippedInstance(player) as FuryInABottle;
				instance.charge += damageDone;

				if (Main.netMode == NetmodeID.MultiplayerClient)
					player.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(shouldRunProjMethods: false);
			}
		}

		private void GainChargeProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player) && !player.GetJumpState<FuryJump>().Available)
			{
				var instance = GetEquippedInstance(player) as FuryInABottle;
				instance.charge += damageDone;

				if (Main.netMode == NetmodeID.MultiplayerClient)
					player.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(shouldRunProjMethods: false);
			}
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetJumpState<FuryJump>().Enable();

			if (charge >= 100)
			{
				player.GetJumpState<FuryJump>().Available = true;
				charge = 0;

				Helpers.Helper.PlayPitched("Effects/Bleep", 1, 0, player.Center);
				for (int k = 0; k < 10; k++)
				{
					Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<Dusts.Cinder>(), 0, -2, 0, Color.Red);
				}
			}
		}
	}

	internal class FuryJump : ExtraJump
	{
		public override Position GetDefaultPosition()
		{
			return AfterBottleJumps;
		}

		public override float GetDurationMultiplier(Player player)
		{
			return 0.6f;
		}

		public override void OnStarted(Player player, ref bool playSound)
		{
			playSound = false;

			Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, 1f);

			Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + new Vector2(-20, 60), Vector2.Zero, ModContent.ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, Main.rand.NextFloat(0.4f, 0.8f));
			Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + new Vector2(20, 60), Vector2.Zero, ModContent.ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, Main.rand.NextFloat(0.4f, 0.8f));

			for (int k = 0; k < 10; k++)
			{
				Dust.NewDustPerfect(player.Center + new Vector2(0, 16), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY.RotatedByRandom(0.2f) * -Main.rand.NextFloat(8), 0, Color.Lerp(Color.Red, Color.Orange, Main.rand.NextFloat()));
			}

			Helpers.Helper.PlayPitched("Magic/FireHit", 0.25f, -0.5f, player.Center);
			Helpers.Helper.PlayPitched("Magic/FireHit", 0.125f, 0.5f, player.Center);
		}
	}
}