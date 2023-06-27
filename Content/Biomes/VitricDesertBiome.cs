﻿using StarlightRiver.Content.Waters;
using StarlightRiver.Core.Systems.LightingSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Biomes
{
	public class VitricDesertBiome : ModBiome
	{
		public override string BestiaryIcon => AssetDirectory.Biomes + "VitricDesertIcon";

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/GlassPassive");

		public override string MapBackground => AssetDirectory.MapBackgrounds + "GlassMap";

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override ModWaterStyle WaterStyle => ModContent.GetInstance<WaterVitric>();

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Desert");
		}

		public override bool IsBiomeActive(Player player)
		{
			Rectangle detectionBox = StarlightWorld.vitricBiome;
			detectionBox.Inflate(Main.screenWidth / 32, Main.screenHeight / 32);

			return detectionBox.Contains((player.position / 16).ToPoint());
		}

		public override void OnInBiome(Player player)
		{
			if (Main.Configuration.Get<bool>("UseHeatDistortion", false) && !Main.npc.Any(n => n.active && n.boss))
			{
				if (!Filters.Scene["GradientDistortion"].IsActive())
				{
					Filters.Scene["GradientDistortion"].GetShader().Shader.Parameters["uZoom"].SetValue(Main.GameViewMatrix.Zoom);
					Filters.Scene.Activate("GradientDistortion").GetShader()
						.UseOpacity(2.5f)
						.UseIntensity(7f)
						.UseProgress(6)
						.UseImage(LightingBuffer.screenLightingTarget.RenderTarget, 0);
				}
			}
			else
			{
				if (Filters.Scene["GradientDistortion"].IsActive())
					Filters.Scene.Deactivate("GradientDistortion");
			}
		}

		public override void OnLeave(Player player)
		{
			if (Filters.Scene["GradientDistortion"].IsActive())
				Filters.Scene.Deactivate("GradientDistortion");
		}
	}

	public class VitricDesertBackground : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/GlassPassive");

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightWorld.vitricBiome.Intersects(new Rectangle((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16));
		}
	}

	public class VitricBossAmbientMusic : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/VitricBossAmbient");

		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && StarlightWorld.VitricBossArena.Contains((player.Center / 16).ToPoint());
		}
	}
}