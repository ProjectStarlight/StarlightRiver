using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushOrb : ModNPC, ILoadable, IHintable
	{
		public static EaseBuilder VoidEase;

		public static List<BossRushOrb> activeBossRushLocks = new();

		public Vector2 originalPos;
		public Vector2 bobbleDirection = Vector2.Zero;

		public bool isPlayingWarp = false;
		public float warpAnimationTimer = 0;

		public float starViewScale = 1;

		public override string Texture => "StarlightRiver/Assets/NPCs/BossRush/BossRushOrb";

		public override void Load()
		{
			StarlightRiverBackground.DrawMapEvent += DrawMap;
			StarlightRiverBackground.DrawOverlayEvent += DrawOverlay;
			StarlightRiverBackground.CheckIsActiveEvent += () => activeBossRushLocks.Count > 0;

			VoidEase = new EaseBuilder();
			VoidEase.AddPoint(new Vector2(0, 0.25f), EaseBuilder.EaseCubicIn);
			VoidEase.AddPoint(new Vector2(45, 1.5f), new CubicBezierEase(0.2f, 1.5f, .8f, 1.5f));
			VoidEase.AddPoint(new Vector2(75, 0.25f), new CubicBezierEase(0.15f, 0.6f, .5f, 1f));
			VoidEase.AddPoint(new Vector2(135, 2f), EaseFunction.EaseCubicInOut);
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 400;
			NPC.width = 42;
			NPC.height = 42;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.knockBackResist = 0f;

			NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/Clink");
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, new Vector3(1, 1, 0.4f));

			NPC.position = originalPos + bobbleDirection;
			bobbleDirection = Vector2.Lerp(Vector2.Zero, bobbleDirection, 0.98f);

			if (isPlayingWarp)
			{
				warpAnimationTimer++;

				starViewScale = VoidEase.Ease(warpAnimationTimer);
			}
			else
			{
				// starViewScale = 1 + 0.5f * (1f - (float)NPC.life / NPC.lifeMax);
				starViewScale = 0.25f;
			}
		}

		public override void OnSpawn(IEntitySource source)
		{
			activeBossRushLocks.Add(this);
			originalPos = NPC.position;
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			hit.HideCombatText = true;
			bobbleDirection += (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * item.knockBack;
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			hit.HideCombatText = true;
			bobbleDirection += (NPC.Center - projectile.Center).SafeNormalize(Vector2.Zero) * projectile.knockBack;
		}

		public override bool CheckDead()
		{
			if (isPlayingWarp && warpAnimationTimer / 300 > 1)
				return true;

			NPC.life = 1;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.active = true;

			isPlayingWarp = true;

			return false;
		}

		public static void DrawOverlay(GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget)
		{
			foreach (BossRushOrb bossRushLock in activeBossRushLocks)
			{
				if (bossRushLock.isPlayingWarp)
				{
					if (bossRushLock.warpAnimationTimer > 75)
					{
						Texture2D distortionMap = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/StarViewWarpMap").Value;

						Effect mapEffect = Filters.Scene["StarViewWarp"].GetShader().Shader;
						mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
						mapEffect.Parameters["distortionMap"].SetValue(distortionMap);
						mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

						Vector2 pos = bossRushLock.NPC.Center - Main.screenPosition;

						mapEffect.Parameters["uIntensity"].SetValue((bossRushLock.warpAnimationTimer - 75) / 60);
						mapEffect.Parameters["uTargetPosition"].SetValue(pos);
						mapEffect.Parameters["uResolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

						Main.spriteBatch.Begin(default, default, default, default, default, mapEffect, Main.GameViewMatrix.TransformationMatrix);

						Main.spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

						Main.spriteBatch.End();
					}
					else
					{
						Effect mapEffect = Filters.Scene["StarMap"].GetShader().Shader;
						mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
						mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

						Main.spriteBatch.Begin(default, default, default, default, default, mapEffect, Main.GameViewMatrix.TransformationMatrix);

						Main.spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

						Main.spriteBatch.End();
					}
				}
			}
		}

		public static void DrawMap(SpriteBatch sb)
		{
			activeBossRushLocks.RemoveAll(x => !x.NPC.active || !Main.npc.Contains(x.NPC));

			foreach (BossRushOrb bossRushLock in activeBossRushLocks)
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;

				Texture2D starView = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/StarView").Value;

				Effect wobble = Filters.Scene["StarViewWobble"].GetShader().Shader;
				wobble.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.05f);
				wobble.Parameters["uIntensity"].SetValue(1f);
				// wobble.Parameters["bobbleAngle"].SetValue(bossRushLock.bobbleDirection.ToRotation());
				// wobble.Parameters["bobbleMag"].SetValue(bossRushLock.bobbleDirection.Length());

				Vector2 pos = bossRushLock.NPC.Center - Main.screenPosition;

				Color color = Color.White;
				color.A = 0;

				int starViewWidth = (int)(600 * bossRushLock.starViewScale);

				sb.End();
				sb.Begin(default, BlendState.Additive, default, default, default, wobble, Main.GameViewMatrix.ZoomMatrix);
				sb.Draw(starView, new Rectangle((int)pos.X - starViewWidth / 2, (int)pos.Y - starViewWidth / 2, starViewWidth, starViewWidth), color);

				sb.End();
				sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}
		}
		public string GetHint()
		{
			return "I SEE YOU.";
		}
	}
}