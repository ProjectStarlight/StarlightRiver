using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Core.Systems.CutawaySystem
{
	public class CutawayHook : HookGroup
	{
		public static List<Cutaway> cutaways = new();

		public static ScreenTarget cutawayTarget = new(DrawCutawayTarget, () => Inside, 1);

		private static bool Inside => cutaways.Any(n => n.fadeTime < 0.95f);

		public override void Load()
		{
			if (Main.dedServ)
				return;

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
			for (int k = 0; k < cutaways.Count; k++)
				cutaways[k].Draw();

			orig(self);
		}

		private void DrawNegative(On_Main.orig_DrawInfernoRings orig, Main self)
		{
			orig(self);

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
				Main.spriteBatch.Begin(default, default, default, default, default, effect);

				Main.spriteBatch.Draw(cutawayTarget.RenderTarget, Vector2.Zero, Color.White);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default);
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