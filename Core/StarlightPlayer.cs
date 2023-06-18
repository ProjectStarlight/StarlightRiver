using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Packets;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
	{
		public bool justHit = false;
		public int lastHit = 0;
		public bool trueInvisible = false;

		public int platformTimer = 0;

		public int pickupTimer = 0; //TODO: Move this into its own thing eventually
		public int maxPickupTimer = 0;
		public NPC pickupTarget;
		public Vector2 oldPickupPos;

		public bool inTutorial;

		public float itemSpeed;
		public float rotation;

		public static List<PlayerTicker> spawners = new();

		public bool shouldSendHitPacket = false;
		public OnHitPacket hitPacket = null;

		public int Timer { get; private set; }

		public override void PreUpdate()
		{
			if (pickupTarget != null)
			{
				if (pickupTimer == 0)
					oldPickupPos = Player.Center;

				pickupTimer++;

				Player.immune = true;
				Player.immuneTime = 5;
				Player.immuneNoBlink = true;

				Player.Center = Vector2.SmoothStep(oldPickupPos, pickupTarget.Center, pickupTimer / 30f);
				if (pickupTimer >= maxPickupTimer)
					pickupTarget = null;
			}
			else
			{
				pickupTimer = 0;
			}

			platformTimer--;

			if (!Player.immune)
			{
				VitricSpike.CollideWithSpikes(Player, out int damage);

				if (damage > 0)
					Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " was impaled by glass shards."), damage, 0);
			}

			foreach (PlayerTicker ticker in spawners.Where(n => n.Active(Player) && Timer % n.TickFrequency == 0))
				ticker.Tick(Player);
		}

		public delegate void ResetEffectsDelegate(StarlightPlayer Player);
		public static event ResetEffectsDelegate ResetEffectsEvent;
		public override void ResetEffects()
		{
			ResetEffectsEvent?.Invoke(this);
			itemSpeed = 1;

			trueInvisible = false;
			shouldSendHitPacket = false;
		}

		public override void PostUpdate()
		{
			PostUpdateEvent.Invoke(Player);

			if (Main.netMode == NetmodeID.MultiplayerClient && Player == Main.LocalPlayer)
				StarlightWorld.visualTimer += (float)Math.PI / 60;
			Timer++;
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			justHit = true;
			lastHit = Timer;
		}

		public delegate void PostUpdateEquipsDelegate(StarlightPlayer Player);
		public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
		public override void PostUpdateEquips()
		{
			PostUpdateEquipsEvent?.Invoke(this);
			justHit = false;
		}

		/// <summary>
		/// This is expected to be run PRIOR to the modify hit hooks so that when we send the data we can send it as it was prior to the edits so when it runs on the other client the modify hooks should be the same at the end
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public void AddHitPacket(Projectile proj, NPC target, NPC.HitModifiers modifiers)
		{
			if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
				hitPacket = new OnHitPacket(Player, proj, target);
		}

		/// <summary>
		/// This is expected to run AFTER the on hit hooks so that if and only if any event during the modify and/or hit hooks wants the data to be synced we will do so
		/// also adds the hit
		/// </summary>
		public void SendHitPacket(NPC.HitInfo hitInfo, int damageDone)
		{
			if (shouldSendHitPacket && hitPacket != null && Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
			{

				hitPacket.addHitInfo(hitInfo, damageDone);
				hitPacket.Send(-1, Main.myPlayer, false);
				shouldSendHitPacket = false;
				hitPacket = null;
			}
		}

		public override void OnEnterWorld()
		{
			ZoomHandler.SetZoomAnimation(Main.GameZoomTarget, 1);

			rotation = 0;

			BossBarOverlay.tracked = null;
			AbilityInventory.shouldReset = true;
			inTutorial = false;

			DummyTile.dummies.Clear();

			if (Main.masterMode && !BossRushSystem.isBossRush)
			{
				UILoader.GetUIState<MessageBox>().Display("WARNING", "Starlight River has unique behavior for its bosses in Master Mode. This behavior is intended to be immensely difficult over anything else, and assumes a high amount of knowldge about " +
					"both the mod and base game. Starlight River Master Mode is not intended for a first playthrough. Starlight River Master Mode is not intended to be fair. Starlight River Master Mode is not intended to be fun for everyone. " +
					"Please remember that the health, both physical and mental, of yourself and those around you is far more important than this game or anything inside of it. Starlight River Master Mode is not liable for any damages, mental or physical, " +
					"incurred by exposure to Starlight River Master Mode, enforced by Issue of Skill, clause 14-A.");
			}
		}

		public override void OnRespawn()
		{
			if (Player == Main.LocalPlayer)
				CameraSystem.Reset();

			rotation = 0;
			inTutorial = false;
		}

		public override void PlayerConnect()
		{
			var packet = new AbilityProgress(Main.myPlayer, Main.LocalPlayer.GetHandler());
			packet.Send(runLocally: false);
		}

		public override float UseTimeMultiplier(Item Item)
		{
			return itemSpeed;
		}

		public void DoubleTapEffects(int keyDir) //TODO: Move this to breacher armor classes
		{
			if (keyDir == (Main.ReversedUpDownArmorSetBonuses ? 1 : 0)) //double tap down
			{
				//Breacher drone
				Projectile spotter = Main.projectile.Where(n => n.owner == Player.whoAmI && n.ModProjectile is SpotterDrone drone2).OrderBy(n => Vector2.Distance(n.Center, Player.Center)).FirstOrDefault();

				if (spotter != default && spotter.ModProjectile is SpotterDrone drone && drone.CanScan)
				{
					BreacherPlayer modPlayer = Player.GetModPlayer<BreacherPlayer>();
					Projectile Projectile = spotter;
					NPC target = Main.npc.Where(n => n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, Main.MouseWorld)).FirstOrDefault();

					if (modPlayer.Charges >= 1 && target != default)
					{
						Helper.PlayPitched("Effects/Chirp" + (Main.rand.Next(2) + 1).ToString(), 0.5f, 0);
						drone.ScanTimer = SpotterDrone.SCAN_TIME;
						drone.Charges = Player.GetModPlayer<BreacherPlayer>().Charges;
						Player.GetModPlayer<BreacherPlayer>().ticks = 0;
					}
				}
			}
		}
	}
}