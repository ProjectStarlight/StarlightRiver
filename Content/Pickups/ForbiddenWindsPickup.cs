﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Tiles.Overgrow;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal class ForbiddenWindsPickup : AbilityPickup, IDrawPrimitive
	{
		private List<Vector2> cache1;
		private List<Vector2> cache2;
		private List<Vector2> cache3;
		private Trail trail1;
		private Trail trail2;
		private Trail trail3;
		private Trail trail4;
		private Trail trail5;
		private Trail trail6;

		private Vector2 point1;
		private Vector2 point2;
		private Vector2 point3;

		public override Asset<Texture2D> Texture => Assets.Abilities.ForbiddenWinds;

		public override Color GlowColor => new(160, 230, 255);

		public ForbiddenWindsPickup() : base(TileType<ForbiddenWindsPickupTile>()) { }

		public override bool CanPickup(Player Player)
		{
			return !Player.GetHandler().Unlocked<Dash>();
		}

		public override void Visuals()
		{
			float sin = 0.2f + Math.Abs((float)Math.Sin(StarlightWorld.visualTimer));
			Dust.NewDustPerfect(new Vector2(Center.X + (float)Math.Sin(StarlightWorld.visualTimer) * 30, Center.Y - 20 + (float)Math.Cos(StarlightWorld.visualTimer) * 10), DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, new Color(100, 200, 255, 0) * sin, 0.16f);

			float sin2 = 0.2f + Math.Abs((float)Math.Cos(StarlightWorld.visualTimer));
			Dust.NewDustPerfect(new Vector2(Center.X + (float)Math.Cos(StarlightWorld.visualTimer) * 25, Center.Y + (float)Math.Sin(StarlightWorld.visualTimer) * 6), DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, new Color(100, 200, 255, 0) * sin2, 0.13f);

			float sin3 = 0.2f + Math.Abs((float)Math.Sin(StarlightWorld.visualTimer));
			Dust.NewDustPerfect(new Vector2(Center.X + (float)Math.Sin(StarlightWorld.visualTimer + 2) * 15, Center.Y + 20 + (float)Math.Cos(StarlightWorld.visualTimer + 2) * 4), DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, new Color(100, 200, 255, 0) * sin3, 0.1f);
		}

		public override void PickupVisuals(int timer)
		{
			Player Player = Main.LocalPlayer;

			if (timer == 1)
			{
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Pickups/ForbiddenWinds")); //start the SFX

				cache1.Clear();
				cache2.Clear();
				cache3.Clear();
			}

			if (timer < 520)
			{
				if (timer < 260)
				{
					float progress = Eases.BezierEase(timer / 260f);
					point1 = Player.Center + new Vector2((float)Math.Sin(progress * 6.28f) * (40 + progress * 80), 100 - Eases.EaseQuadIn(progress) * 320);
					point2 = Player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3) * (40 + progress * 80), 100 - Eases.EaseQuadIn(progress) * 320);
					point3 = Player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3 * 2) * (40 + progress * 80), 100 - Eases.EaseQuadIn(progress) * 320);
				}

				if (timer >= 260 && timer <= 380)
				{
					float progress = 1 - Eases.BezierEase((timer - 260) / 120f);
					point1 = Player.Center + new Vector2((float)Math.Sin(progress * 6.28f) * progress * 120, progress * -220);
					point2 = Player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3) * progress * 120, progress * -220);
					point3 = Player.Center + new Vector2((float)Math.Sin(progress * 6.28f + 6.28f / 3 * 2) * progress * 120, progress * -220);
				}

				if (cache1 != null && timer > 100 && timer < 380)
				{
					float fade = Math.Clamp((timer - 100) / 120f, 0, 1);
					Dust.NewDustPerfect(cache1[Main.rand.Next(20, 100)], DustType<Dusts.PixelatedEmber>(), Vector2.UnitY * Main.rand.NextFloat(-1, -0.5f), 0, new Color(20, 35, 35, 0) * fade, 0.3f * fade);
					Dust.NewDustPerfect(cache2[Main.rand.Next(20, 100)], DustType<Dusts.PixelatedEmber>(), Vector2.UnitY * -1, 0, new Color(20, 35, 35, 0) * fade, 0.3f * fade);
					Dust.NewDustPerfect(cache3[Main.rand.Next(20, 100)], DustType<Dusts.PixelatedEmber>(), Vector2.UnitY * -1, 0, new Color(20, 35, 35, 0) * fade, 0.3f * fade);
				}

				ManageCache(ref cache1, point1);
				ManageCache(ref cache2, point2);
				ManageCache(ref cache3, point3);

				ManageTrail(ref trail1, cache1);
				ManageTrail(ref trail2, cache2);
				ManageTrail(ref trail3, cache3);
				ManageTrail(ref trail4, cache1, true);
				ManageTrail(ref trail5, cache2, true);
				ManageTrail(ref trail6, cache3, true);
			}

			if (timer >= 500)
			{
				Player.GetModPlayer<StarlightPlayer>().pickupHoldsPlayer = false;

				Player.controlUp = true;
				Player.controlLeft = false;
				Player.controlRight = false;
				Player.controlDown = false;

				float timeRel = (timer - 500) / 150f;
				Dust.NewDust(Player.position, Player.width, Player.height, DustType<Content.Dusts.Air>(), 0, 0, 0, default, 0.3f);
				//Filters.Scene.Activate("Shockwave", Player.Center).GetShader().UseProgress(2f).UseIntensity(100).UseDirection(new Vector2(0.005f + timeRel * 0.5f, 1 * 0.02f - timeRel * 0.02f));
			}

			if (timer == 500)
			{
				SoundEngine.PlaySound(SoundID.Item104);
				SoundEngine.PlaySound(SoundID.Item45);

				for (int k = 0; k < 100; k++)
				{
					Dust.NewDustPerfect(Player.Center, DustType<Dusts.PixelatedEmber>(), Main.rand.NextVector2Circular(20, 20), 0, new Color(20, 35, 35, 0), Main.rand.NextFloat(0.5f));
				}

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Player.Center, DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(10, 10), 0, new Color(20, 35, 35, 0), Main.rand.NextFloat(0.5f));
				}

				Player.GetHandler().Stamina = Player.GetHandler().StaminaMax;

				if (Player.GetHandler().GetAbility<Dash>(out Dash dash))
				{
					// We have to hard set this since it would only be triggered by ProcessTriggers otherwise
					dash.Dir = Vector2.UnitY * -1;
					dash.Activate(Player.GetHandler());
				}
			}

			if (timer == 569) //popup + codex entry
			{
				if (StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys().Count <= 0)
					KeybindHelper.OpenKeybindsWithHelp();

				TutorialManager.ActivateTutorial("DashAbility");

				//Filters.Scene.Activate("Shockwave", Player.Center).GetShader().UseProgress(0f).UseIntensity(0);
				//Filters.Scene.Deactivate("Shockwave");
			}

			// audio fade shenanigans
			Main.musicFade[Main.curMusic] = timer < 500 ? 0 : (timer - 500) / 70f;
		}

		public override void PickupEffects(Player player)
		{
			player.GetHandler().Unlock<Dash>();

			player.GetModPlayer<HintPlayer>().SetHintState("PreCeiros");

			player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 650;
			player.GetModPlayer<StarlightPlayer>().inTutorial = true;
			player.AddBuff(BuffID.Featherfall, 660);
		}

		private void ManageCache(ref List<Vector2> cache, Vector2 point)
		{
			if (cache == null || cache.Count < 120)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 120; i++)
				{
					cache.Add(Center + new Vector2(0, 100));
				}
			}

			cache.Add(point);

			while (cache.Count > 120)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail(ref Trail trail, List<Vector2> cache, bool wide = false)
		{
			if (wide)
			{
				if (trail is null || trail.IsDisposed)
				{
					trail = new Trail(Main.instance.GraphicsDevice, 120, new NoTip(), factor => Math.Min((float)Math.Sin(factor * 3.14f) * 24, 24), factor =>
									{
										if (factor.X == 1 || factor.X == 0)
											return Color.Transparent;

										int time = Main.LocalPlayer.GetModPlayer<StarlightPlayer>().pickupTimer;
										float mul = 1;

										if (time < 100 + factor.X * 120)
											mul = 0;

										float relTime = time - 100 - factor.X * 120;
										if (relTime < 120)
											mul = Math.Clamp(relTime / 120f, 0, 1);

										if (time > 600)
											mul = 1 - (time - 600) / 20f;

										return new Color(140, 150 + (int)(105 * factor.X), 255) * (float)Math.Sin(factor.X * 3.14f) * mul * 0.06f;
									});
				}
			}
			else
			{
				if (trail is null || trail.IsDisposed)
				{
					trail = new Trail(Main.instance.GraphicsDevice, 120, new NoTip(), factor => Math.Min((float)Math.Sin(factor * 3.14f) * 13, 13), factor =>
									{
										if (factor.X == 1 || factor.X == 0)
											return Color.Transparent;

										int time = Main.LocalPlayer.GetModPlayer<StarlightPlayer>().pickupTimer;
										float mul = 1;

										if (time < 100 + factor.X * 120)
											mul = 0;

										float relTime = time - 100 - factor.X * 120;
										if (relTime < 120)
											mul = Math.Clamp(relTime / 120f, 0, 1);

										if (time > 600)
											mul = 1 - (time - 600) / 20f;

										return new Color(140, 150 + (int)(105 * factor.X), 255) * (float)Math.Sin(factor.X * 3.14f) * mul;
									});
				}
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

			if (effect is null)
				return;

			var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
			Matrix view = Matrix.Identity;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.01f);
			effect.Parameters["repeats"].SetValue(4f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail1?.Render(effect);
			trail2?.Render(effect);
			trail3?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

			trail4?.Render(effect);
			trail5?.Render(effect);
			trail6?.Render(effect);
		}

		public override void PostDraw(Color lightColor)
		{
			base.PostDraw(lightColor);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", DrawPrimitives);
		}

		public string GetHint()
		{
			return "A dense conflux of Starlight energy... could this be the tangle Alican mentioned?";
		}
	}

	public class ForbiddenWindsPickupTile : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<ForbiddenWindsPickup>();

		public override string Texture => AssetDirectory.Invisible;
	}

	[SLRDebug]
	public class WindsTileItem : QuickTileItem
	{
		public WindsTileItem() : base("Forbidden Winds", "{{Debug}} placer for ability pickup", "ForbiddenWindsPickupTile", -1) { }

		public override string Texture => "StarlightRiver/Assets/Abilities/ForbiddenWindsPreview";
	}
}