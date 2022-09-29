﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.CustomHooks
{
	public class CutawayHandler : HookGroup
	{
		public static List<Cutaway> cutaways = new List<Cutaway>();
		private static bool inside => cutaways.Any(n => n.fadeTime < 0.95f);

		public static RenderTarget2D cutawayTarget;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.SetDisplayMode += RefreshCutawayTarget;
			On.Terraria.Main.DrawInfernoRings += DrawNegative;
			On.Terraria.Main.DrawDust += DrawPositive;
			On.Terraria.WorldGen.SaveAndQuit += ClearCutaways;

			On.Terraria.Main.CheckMonoliths += DrawCutawayTarget;

			Main.QueueMainThreadAction(() =>
			{
				cutawayTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, default, default, default, RenderTargetUsage.PreserveContents);
			});
		}

		public override void Unload()
		{
			cutaways = null;
			cutawayTarget = null;
		}

		private void RefreshCutawayTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
		{
			if (!Main.gameInactive && (width != Main.screenWidth || height != Main.screenHeight))
				cutawayTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height, false, default, default, default, RenderTargetUsage.PreserveContents);

			orig(width, height, fullscreen);
		}

		private void ClearCutaways(On.Terraria.WorldGen.orig_SaveAndQuit orig, Action callback)
		{
			cutaways.Clear();
			orig(callback);
		}

		private void DrawPositive(On.Terraria.Main.orig_DrawDust orig, Main self)
		{
			for (int k = 0; k < cutaways.Count; k++)
				cutaways[k].Draw();

			orig(self);
		}

		private void DrawNegative(On.Terraria.Main.orig_DrawInfernoRings orig, Main self)
		{
			orig(self);

			if (inside)
			{
				var activeCutaway = cutaways.FirstOrDefault(n => n.fadeTime < 0.95f);

				var effect = Filters.Scene["Negative"].GetShader().Shader;

				if (effect is null)
					return;

				effect.Parameters["sampleTexture"].SetValue(cutawayTarget);
				effect.Parameters["uColor"].SetValue((Color.Black).ToVector3());
				effect.Parameters["opacity"].SetValue(1 - activeCutaway.fadeTime);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, effect);

				Main.spriteBatch.Draw(cutawayTarget, Vector2.Zero, Color.White);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default);
			}
		}

		private void DrawCutawayTarget(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			Main.spriteBatch.Begin();

			Main.graphics.GraphicsDevice.SetRenderTarget(cutawayTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			for(int k = 0; k < cutaways.Count; k++)
			{
				if (cutaways[k].fadeTime < 0.95f)
				{
					cutaways[k].Draw(1);
				}
			}

			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.SetRenderTarget(null);

			orig();
		}

		public static void NewCutaway(Cutaway cutaway)
		{
			cutaways.Add(cutaway);
		}
	}
}
