﻿using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Core.Systems.CutawaySystem
{
	public class CutawayHook : HookGroup
	{
		public static List<Cutaway> cutaways;

		public static ScreenTarget cutawayTarget;

		// We track this so we can disable these in subworlds
		private static Mod subLib;

		private static bool Inside => cutaways?.Any(n => n.fadeTime < 0.95f) ?? false;

		public static bool InSubworld => (bool)(subLib?.Call("AnyActive") ?? false);

		public override void Load()
		{
			if (Main.dedServ)
				return;

			cutaways = new();
			cutawayTarget = new(DrawCutawayTarget, () => Inside, 1);

			ModLoader.TryGetMod("SubworldLibrary", out subLib);

			On_Main.DrawInfernoRings += DrawNegative;
			On_Main.DrawDust += DrawPositive;
			On_WorldGen.SaveAndQuit += ClearCutaways;
		}

		public override void Unload()
		{
			cutaways = null;
			cutawayTarget = null;
		}

		private void ClearCutaways(On_WorldGen.orig_SaveAndQuit orig, Action callback)
		{
			cutaways.Clear();
			orig(callback);
		}

		private void DrawPositive(On_Main.orig_DrawDust orig, Main self)
		{
			if (!InSubworld)
			{
				for (int k = 0; k < cutaways.Count; k++)
					cutaways[k].Draw();
			}

			orig(self);
		}

		private void DrawNegative(On_Main.orig_DrawInfernoRings orig, Main self)
		{
			orig(self);

			if (StarlightRiver.debugMode || InSubworld)
				return;

			if (Inside)
			{
				Cutaway activeCutaway = cutaways.FirstOrDefault(n => n.fadeTime < 0.95f);

				Effect effect = Filters.Scene["Negative"].GetShader().Shader;

				if (effect is null)
					return;

				effect.Parameters["sampleTexture"].SetValue(cutawayTarget.RenderTarget);
				effect.Parameters["uColor"].SetValue(Color.Black.ToVector3());
				effect.Parameters["opacity"].SetValue(1 - activeCutaway.fadeTime);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect);

				Main.spriteBatch.Draw(cutawayTarget.RenderTarget, Vector2.Zero, Color.White);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default);
			}
		}

		private static void DrawCutawayTarget(SpriteBatch sb)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			for (int k = 0; k < cutaways.Count; k++)
			{
				if (cutaways[k].fadeTime < 0.95f)
					cutaways[k].Draw(1);
			}
		}

		public static void NewCutaway(Cutaway cutaway)
		{
			cutaways.Add(cutaway);
		}
	}
}