using Microsoft.Xna.Framework;
using StarlightRiver.Codex.Entries;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using StarlightRiver.Content.Waters;

namespace StarlightRiver.Content.Biomes
{
	public class VitricDesertBiome : ModBiome
	{
		public override string BestiaryIcon => AssetDirectory.Biomes + "VitricDesertIcon";

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/GlassPassive");

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Desert");
		}

		public override bool IsBiomeActive(Player player)
		{
			return StarlightWorld.VitricBiome.Contains((player.position / 16).ToPoint());
		}

		public override ModWaterStyle WaterStyle => ModContent.GetInstance<WaterVitric>();

		public override void OnInBiome(Player player)
		{
			if (Main.Configuration.Get<bool>("UseHeatDistortion", false))
			{
				if (!Filters.Scene["GradientDistortion"].IsActive())
				{
					Filters.Scene["GradientDistortion"].GetShader().Shader.Parameters["uZoom"].SetValue(Main.GameViewMatrix.Zoom);
					Filters.Scene.Activate("GradientDistortion").GetShader()
						.UseOpacity(2.5f)
						.UseIntensity(7f)
						.UseProgress(6)
						.UseImage(StarlightRiver.LightingBufferInstance.ScreenLightingTexture, 0);
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

		public override void OnEnter(Player player)
		{
			Helper.UnlockCodexEntry<VitricEntry>(player);
		}	
	}

	public class VitricDesertBackground : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/GlassPassive");

		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("StarlightRiver/BlankBG");

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightWorld.VitricBiome.Intersects(new Rectangle((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16));
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
