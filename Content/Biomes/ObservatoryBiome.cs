using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.Bestiary;
using StarlightRiver.Content.Events;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Biomes
{
	internal class ObservatoryBiome : ModBiome
	{
		public static float fade;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/StarBird");

		public override SceneEffectPriority Priority => SceneEffectPriority.Event;

		public override void Load()
		{
			On_Main.DrawBackgroundBlackFill += DrawRiver;
			StarlightRiverBackground.CheckIsActiveEvent += () => !Main.gameMenu && IsSceneEffectActive(Main.LocalPlayer);
			StarlightRiverBackground.DrawMapEvent += DrawOverlayMap;
			StarlightRiverBackground.DrawOverlayEvent += DrawOverlay;
		}

		public override void OnInBiome(Player player)
		{
			bool biomeCondition = ObservatorySystem.IsInObservatory(player);

			if (biomeCondition && fade < 1)
				fade += 0.02f;

			if (!biomeCondition && fade > 0)
				fade -= 0.05f;
		}

		private void DrawOverlayMap(SpriteBatch sb)
		{
			if (!Main.gameMenu && IsSceneEffectActive(Main.LocalPlayer))
			{
				Texture2D tex = Assets.Noise.PerlinNoise.Value;

				sb.End();
				sb.Begin(default, default, SamplerState.PointWrap, default, default);

				sb.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Rectangle((int)Main.GameUpdateCount / 3, 0, tex.Width, tex.Height), Color.White * fade * 0.2f);

				Texture2D glowTex = Assets.Masks.Glow.Value;

				float opacity = fade;
				Color color = Color.Black * opacity;

				sb.Draw(glowTex, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), null, color, 0, glowTex.Size() / 2f, opacity * 10f, 0, 0);
			}
		}

		private void DrawRiver(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
		{
			orig(self);

			if (!Main.gameMenu && IsSceneEffectActive(Main.LocalPlayer))
				Main.spriteBatch.Draw(StarlightRiverBackground.starsTarget.RenderTarget, Vector2.Zero, Color.White * fade);
		}

		private void DrawOverlay(GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget)
		{
			if (!Main.gameMenu && IsSceneEffectActive(Main.LocalPlayer))
			{
				SpriteBatch spriteBatch = Main.spriteBatch;

				Effect mapEffect = ShaderLoader.GetShader("StarMap").Value;

				if (mapEffect != null)
				{
					mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
					mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

					spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, mapEffect, Main.GameViewMatrix.TransformationMatrix);

					spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

					spriteBatch.End();
				}
			}

			if (StarlightRiver.debugMode && false)
			{
				Main.spriteBatch.Begin();
				Rectangle target = ObservatorySystem.ObservatoryRoomWorld;
				target.Offset((-Main.screenPosition).ToPoint());
				Main.spriteBatch.Draw(Assets.MagicPixel.Value, target, Color.Green * 0.25f);

				target = ObservatorySystem.MainStructureWorld;
				target.Offset((-Main.screenPosition).ToPoint());
				Main.spriteBatch.Draw(Assets.MagicPixel.Value, target, Color.Orange * 0.25f);

				target = ObservatorySystem.SideStructureWorld;
				target.Offset((-Main.screenPosition).ToPoint());
				Main.spriteBatch.Draw(Assets.MagicPixel.Value, target, Color.Red * 0.25f);
				Main.spriteBatch.End();
			}
		}

		public override bool IsBiomeActive(Player player)
		{
			return ObservatorySystem.IsInObservatory(player) || fade > 0;
		}
	}

	internal class ObservatorySystem : ModSystem
	{
		public static Rectangle observatoryRoom;
		public static bool observatoryOpen;
		public static bool pylonAppearsOn;

		public static Rectangle ObservatoryRoomWorld => new(observatoryRoom.X * 16, observatoryRoom.Y * 16, observatoryRoom.Width * 16, observatoryRoom.Height * 16);
		public static Rectangle MainStructureWorld => new(observatoryRoom.X * 16, observatoryRoom.Y * 16 + 7 * 16, observatoryRoom.Width * 16, observatoryRoom.Height * 16 * 2);
		public static Rectangle SideStructureWorld => new(observatoryRoom.X * 16 - 9 * 16, observatoryRoom.Y * 16 + 11 * 16, 9 * 16, 10 * 16);

		public static bool IsInMainStructure(Player player)
		{
			return player.Hitbox.Intersects(MainStructureWorld) || player.Hitbox.Intersects(SideStructureWorld);
		}

		public static bool IsInObservatory(Player player)
		{
			return IsInMainStructure(player) || player.Hitbox.Intersects(ObservatoryRoomWorld);
		}

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
			ObservatoryBiome biome = ModContent.GetInstance<ObservatoryBiome>();

			if (biome.IsBiomeActive(Main.LocalPlayer))
			{
				tileColor = Color.Lerp(tileColor, new Color(30, 40, 70), ObservatoryBiome.fade);
				backgroundColor = Color.Lerp(backgroundColor, new Color(2, 36, 62), ObservatoryBiome.fade);
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["observatoryRoom"] = observatoryRoom;
			tag["observatoryOpen"] = observatoryOpen;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			observatoryRoom = tag.Get<Rectangle>("observatoryRoom");
			observatoryOpen = tag.GetBool("observatoryOpen");
			pylonAppearsOn = observatoryOpen;
		}
	}
}