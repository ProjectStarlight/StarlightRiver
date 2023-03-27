using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Codex.Entries;
using StarlightRiver.Content.Foregrounds;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Pickups
{
	class InfusionSlotPickup : AbilityPickup
	{
		public override string Texture => "StarlightRiver/Assets/Invisible";

		public override Color GlowColor => Color.Black;

		public override bool CanPickup(Player Player)
		{
			return Player.GetHandler().InfusionLimit <= 0;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Unnerving Device");
		}

		public override void SafeSetDefaults()
		{
			NPC.behindTiles = true;
		}

		public override void Visuals()
		{

		}

		public override void PickupVisuals(int timer)
		{
			Player Player = Main.LocalPlayer;

			//blood spray and sounds
			if (timer == 15 || timer > 120 && timer < 220 && timer % 10 == 8)
			{
				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Player.Center, DustID.Blood, Vector2.UnitY.RotatedBy(timer / 10f - 1.57f).RotatedByRandom(0.25f) * Main.rand.NextFloat(10));
				}

				Player.headPosition = Vector2.UnitX.RotatedByRandom(3.14f) * Main.rand.NextFloat(5);
				Player.bodyPosition = Vector2.UnitX.RotatedByRandom(3.14f) * Main.rand.NextFloat(5);
				Player.legPosition = Vector2.UnitX.RotatedByRandom(3.14f) * Main.rand.NextFloat(5);

				Helper.PlayPitched("Impale", 1, Main.rand.NextFloat(0.6f, 0.9f), Player.Center);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.PlayerHit, Player.Center);
			}

			if (timer >= 15)
			{
				CreepyVignette.visible = true;
				CreepyVignette.opacityMult = 0.5f;
				CreepyVignette.holeSize = Vector2.One * 400;
			}

			if (timer == 360)
				Helper.PlayPitched("Impale", 1, Main.rand.NextFloat(0.6f, 0.9f), Player.Center); //placeholder sound

			if (timer >= 360)
			{
				CreepyVignette.offset = Vector2.SmoothStep(Vector2.Zero, new Vector2(-Main.screenWidth / 2 + 100, -Main.screenHeight / 2 + 300), Math.Min(1, (timer - 360) / 120f));
				CreepyVignette.opacityMult = 0.5f + Math.Min(1, (timer - 360) / 120f) * 0.25f;
				CreepyVignette.holeSize = Vector2.One * (400 - Math.Min(1, (timer - 360) / 120f) * 64);
			}

			if (timer < 360)
			{
				if (Main.playerInventory)
				{
					Player.controlInv = true;
					Player.releaseInventory = true;
					Main.playerInventory = false;
				}
			}

			if (timer >= 360)
			{
				if (!Main.playerInventory)
				{
					Player.ToggleInv();
					Main.playerInventory = true;
				}
			}

			if (timer > 530)
			{
				CreepyVignette.visible = false;
				CreepyVignette.offset = new Vector2(-Main.screenWidth / 2 + 100, -Main.screenHeight / 2 + 300);
				CreepyVignette.opacityMult = 0.75f - Math.Min(1, (timer - 530) / 30f) * 0.75f;
				CreepyVignette.holeSize = Vector2.One * (336 + Math.Min(1, (timer - 530) / 30f) * 512);
			}

			if (timer == 559)
			{
				UILoader.GetUIState<TextCard>().Display("Mysterious Technology", "What has it done to you?", time: 360);
				Helper.UnlockCodexEntry<InfusionEntry>(Main.LocalPlayer);

				Player.headPosition = Vector2.Zero;
				Player.bodyPosition = Vector2.Zero;
				Player.legPosition = Vector2.Zero;
			}
		}

		public override void PickupEffects(Player Player)
		{
			Player.GetHandler().InfusionLimit = 1;
			Player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 560;
		}

		public override void DrawBehind(int index)
		{
			//Main.instance.DrawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Player Player = Main.LocalPlayer;
			int timer = Player.GetModPlayer<StarlightPlayer>().pickupTimer;

			if (timer < 1 || timer > 559)
				return;

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Abilities/BrassSpike").Value;
			var origin = new Vector2(0, tex.Height / 2);

			float progressFirst = Math.Min(1, timer / 12f);

			if (timer > 300)
				progressFirst = 1 - (timer - 300) / 60f;

			Vector2 positionFirst = Player.Center + Vector2.SmoothStep(Vector2.UnitY * 100, Vector2.Zero, progressFirst) - screenPos;

			spriteBatch.Draw(tex, positionFirst, null, Color.White, (float)Math.PI * 0.5f, origin, 1, 0, 0);

			if (timer > 120)
			{
				for (int k = 0; k < 10; k++)
				{
					int relTime = timer - (120 + k * 10);
					int relTime2 = timer - (120 + k * 2);
					float progress = Math.Min(1, relTime / 12f);

					if (relTime2 > 180)
						progress = 1 - (relTime2 - 180) / 60f;

					Vector2 position = Player.Center + Vector2.SmoothStep(Vector2.UnitX.RotatedBy(k) * 140, Vector2.Zero, progress) - screenPos;

					if (progress > 0)
					{
						spriteBatch.Draw(tex, position, null, Color.White, k, origin, 1, 0, 0);

						if (relTime2 < 140)
							Dust.NewDustPerfect(Player.Center + Vector2.UnitX.RotatedBy(k) * Main.rand.Next(80, 140), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(k + 3.14f) * Main.rand.NextFloat(4), 0, new Color(255, 200, 100), 0.25f);
					}
				}
			}
		}
	}
}