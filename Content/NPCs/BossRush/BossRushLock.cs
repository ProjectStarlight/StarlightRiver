using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushLock : ModNPC, ILoadable
	{
		public static List<BossRushLock> activeBossRushLocks = new List<BossRushLock>();

		public override string Texture => "StarlightRiver/Assets/NPCs/BossRush/BossRushLock";

		public override void Load()
		{
			StarlightRiverBackground.DrawMapEvent += DrawMap;
			StarlightRiverBackground.DrawOverlayEvent += DrawOverlay;
			StarlightRiverBackground.CheckIsActiveEvent += () => activeBossRushLocks.Count > 0;
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
		}

		public override void OnSpawn(IEntitySource source)
		{
			activeBossRushLocks.Add(this);
		}

		public static void DrawOverlay(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget)
		{
			foreach (BossRushLock bossRushLock in activeBossRushLocks)
			{
				Texture2D distortionMap = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/StarViewWarpMap").Value;

				Effect mapEffect = Filters.Scene["StarViewWarp"].GetShader().Shader;
				mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
				mapEffect.Parameters["distortionMap"].SetValue(distortionMap);
				mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

				Vector2 pos = bossRushLock.NPC.position - Main.screenPosition + Vector2.UnitY * 150;

				mapEffect.Parameters["uIntensity"].SetValue(0);
				mapEffect.Parameters["uTargetPosition"].SetValue(pos);
				mapEffect.Parameters["uResolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

				Main.spriteBatch.Begin(default, default, default, default, default, mapEffect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

				Main.spriteBatch.End();
			}
		}

		public static void DrawMap(SpriteBatch sb)
		{
			activeBossRushLocks.RemoveAll(x => !x.NPC.active);

			foreach (BossRushLock bossRushLock in activeBossRushLocks)
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;

				Texture2D starView = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/DotTell").Value;

				Effect wobble = Filters.Scene["StarViewWobble"].GetShader().Shader;
				wobble.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.05f);
				wobble.Parameters["uIntensity"].SetValue(1f);

				Vector2 pos = bossRushLock.NPC.Center - Main.screenPosition;

				Color color = Color.White;
				color.A = 0;

				sb.End();
				sb.Begin(default, BlendState.Additive, default, default, default, wobble, Main.GameViewMatrix.ZoomMatrix);
				sb.Draw(starView, new Rectangle((int)pos.X - 300, (int)pos.Y - 300 - 150, 600, 600), color);

				sb.End();
				sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}
		}
	}
}