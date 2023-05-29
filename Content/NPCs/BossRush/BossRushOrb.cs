using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushOrb : ModNPC, ILoadable
	{
		const float MAX_CRACK_ANIMATION = 180;

		public static EaseBuilder VoidEase;

		public static List<BossRushOrb> activeBossRushLocks = new();

		public Vector2 originalPos;

		public bool isPlayingCrack = false;
		public bool isPlayingWarp = false;
		public float crackAnimationTimer = 0;
		public float warpAnimationTimer = 0;

		public float starViewScale = 1;

		public float CrackAnimationProgress => crackAnimationTimer / MAX_CRACK_ANIMATION;

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
			VoidEase.AddPoint(new Vector2(105, 0.25f), EaseBuilder.EaseCubicIn);
			VoidEase.AddPoint(new Vector2(165, 2f), EaseFunction.EaseCubicInOut);
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
			if (isPlayingWarp)
			{
				if (warpAnimationTimer == 0)
				{
					for (int i = 0; i < 100; i++)
					{
						Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(20, 20);

						Dust.NewDustPerfect(pos, DustID.Obsidian, Main.rand.NextVector2Circular(5, 0) - new Vector2(0, Main.rand.NextFloat(5)));
					}
				}

				if (warpAnimationTimer < 60)
				{
					Filters.Scene.Activate("Shockwave", NPC.Center).GetShader().UseProgress(2).UseIntensity(60.1f - warpAnimationTimer).UseDirection(new Vector2(warpAnimationTimer / 90, warpAnimationTimer / 90));
				}
				else
				{
					Filters.Scene.Deactivate("Shockwave");
				}

				warpAnimationTimer++;

				starViewScale = VoidEase.Ease(warpAnimationTimer);

				if (warpAnimationTimer / 300 > 1)
					NPC.Kill();
			}
			else if (isPlayingCrack)
			{
				if (CrackAnimationProgress >= 1)
					isPlayingWarp = true;

				crackAnimationTimer++;

				NPC.velocity = Vector2.Lerp((originalPos - NPC.position) * 0.01f, NPC.velocity, 0.95f);
				NPC.Center += Main.rand.NextVector2Circular(2, 2) * (1 + CrackAnimationProgress * 0.5f);
			}
			else
			{
				if (NPC.life < NPC.lifeMax / 2)
				{
					NPC.Center += Main.rand.NextVector2Circular(2, 2) * (1f - NPC.life / (NPC.lifeMax / 2f));
				}

				// starViewScale = 1 + 0.5f * (1f - (float)NPC.life / NPC.lifeMax);
				starViewScale = 0.25f;

				NPC.velocity = Vector2.Lerp((originalPos - NPC.position) * 0.1f, NPC.velocity, 0.9f);
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

			if (NPC.velocity.Length() < 3)
			{
				NPC.velocity += (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * item.knockBack * 0.5f;
			}
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			hit.HideCombatText = true;

			if (NPC.velocity.Length() < 3)
			{
				NPC.velocity += (NPC.Center - projectile.Center).SafeNormalize(Vector2.Zero) * projectile.knockBack * 0.5f;
			}
		}

		public override bool CheckDead()
		{
			if (isPlayingWarp && warpAnimationTimer / 300 > 1)
				return true;

			NPC.life = 1;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.active = true;

			isPlayingCrack = true;

			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			// DrawBubbleCracks();

			if (isPlayingWarp)
			{
				return false;
			}

			if (isPlayingCrack)
			{
				DrawBubbleCracks(0.2f + CrackAnimationProgress * 0.8f);
				return false;
			}

			if (NPC.life < NPC.lifeMax / 2)
			{
				DrawBubbleCracks((1f - NPC.life / (NPC.lifeMax / 2f)) * 0.2f);
				return false;
			}

			return true;
		}

		private void DrawBubbleCracks(float crackProgress)
		{
			// float crackProgress = 0.5f;

			Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, NPC.Center - Main.screenPosition, null, Color.White, 0, ModContent.Request<Texture2D>(Texture).Size() * 0.5f, NPC.scale, SpriteEffects.None, 0);

			Color color = Color.LightGoldenrodYellow * crackProgress;
			Effect crack = Filters.Scene["MagmaCracks"].GetShader().Shader;
			crack.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Glassweaver + "BubbleCrackMap").Value);
			crack.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Glassweaver + "BubbleCrackProgression").Value);
			crack.Parameters["uTime"].SetValue(crackProgress);
			crack.Parameters["drawColor"].SetValue((color * 1.5f).ToVector4());
			crack.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, 128, 128));
			crack.Parameters["texSize"].SetValue(ModContent.Request<Texture2D>(Texture).Value.Size());

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, crack, Main.GameViewMatrix.TransformationMatrix);

			Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, NPC.Center - Main.screenPosition, null, Color.Black, 0, ModContent.Request<Texture2D>(Texture).Size() * 0.5f, NPC.scale, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			float glowMult = (0.75f + 0.5f * (float)Math.Pow(Math.Sin(Main.GameUpdateCount / 20f), 2)) * 2f;

			if (isPlayingCrack)
				glowMult = 1;

			Color glowColor = color * glowMult;

			Main.spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.Keys + "StarAlpha").Value, NPC.Center - Main.screenPosition, null, glowColor, 0, ModContent.Request<Texture2D>(AssetDirectory.Keys + "StarAlpha").Size() * 0.5f, NPC.scale * 1.25f, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.Keys + "StarAlpha").Value, NPC.Center - Main.screenPosition, null, glowColor, MathHelper.ToRadians(45), ModContent.Request<Texture2D>(AssetDirectory.Keys + "StarAlpha").Size() * 0.5f, NPC.scale * 0.9f, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
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
	}
}