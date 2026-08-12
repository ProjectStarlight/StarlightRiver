using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Systems.CutawaySystem
{
	internal class CutawayHandler : ModSystem
	{
		public static bool created = false;

		public static List<Cutaway> cutaways;

		public static ScreenTarget cutawayTarget;

		private static Mod subLib;

		public static Cutaway cathedralOverlay;
		public static Cutaway forgeOverlay;
		public static Cutaway templeOverlay;

		private static bool Inside => cutaways?.Any(n => n.fadeTime < 0.95f) ?? false;

		public static bool InSubworld => subLib?.Call("Current") != null;

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

		public static void CreateCutaways()
		{
			cutaways.Clear();

			// Dont create in subworlds
			if (InSubworld)
				return;

			// Auroracle temple overlay
			cathedralOverlay = new Cutaway(Assets.Bosses.SquidBoss.CathedralOver, StarlightWorld.squidBossArena.TopLeft() * 16)
			{
				Inside = CheckForSquidArena
			};
			cutaways.Add(cathedralOverlay);

			// Glassweaver forge overlay
			forgeOverlay = new Cutaway(Assets.Overlay.ForgeOverlay, StarlightWorld.GlassweaverArena.TopLeft() + new Vector2(-2, 2) * 16)
			{
				Inside = (n) =>
				{
					Rectangle arena = StarlightWorld.GlassweaverArena;
					arena.Y += 4 * 16;
					arena.Height -= 4 * 16;
					return arena.Intersects(n.Hitbox);
				}
			};
			cutaways.Add(forgeOverlay);

			// Vitric temple overlay
			Point16 dimensions = StructureHelper.API.Generator.GetStructureDimensions("Structures/VitricTempleNew", StarlightRiver.Instance);
			Vector2 templePos = new Vector2(StarlightWorld.vitricBiome.Center.X - dimensions.X / 2, StarlightWorld.vitricBiome.Center.Y - 1) * 16;
			templePos.Y -= 9;
			templeOverlay = new Cutaway(Assets.Overlay.TempleOverlay, templePos)
			{
				Inside = (n) => n.InModBiome<VitricTempleBiome>()
			};
			cutaways.Add(templeOverlay);
		}

		/// <summary>
		/// Condition for the auroracle overlay to be seen
		/// </summary>
		/// <param name="Player"></param>
		/// <returns>If the auroracle arena overlay should be active</returns>
		private static bool CheckForSquidArena(Player Player)
		{
			if (WorldGen.InWorld((int)Main.LocalPlayer.Center.X / 16, (int)Main.LocalPlayer.Center.Y / 16))
			{
				Tile tile = Framing.GetTileSafely((int)Main.LocalPlayer.Center.X / 16, (int)Main.LocalPlayer.Center.Y / 16);

				if (tile != null)
				{
					return
						tile.WallType == ModContent.WallType<AuroraBrickWall>() &&
						!Main.LocalPlayer.GetModPlayer<StarlightPlayer>().trueInvisible;
				}
			}

			return false;
		}

		private void ClearCutaways(On_WorldGen.orig_SaveAndQuit orig, Action callback)
		{
			cutaways.Clear();
			orig(callback);
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

				Effect effect = ShaderLoader.GetShader("Negative").Value;

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

		public override void PostUpdateEverything()
		{
			if (!Main.dedServ && !created)
			{
				CreateCutaways();
				created = true;
			}
		}

		public override void OnWorldLoad()
		{
			created = false;
		}

		public override void OnWorldUnload()
		{
			created = false;
		}
	}
}