using Terraria.ModLoader.Config;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.ComponentModel;

namespace StarlightRiver.Configs
{
    public class GraphicsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Screenshake")]
        [Tooltip("Modifies the intensity of screen shake effects")]
        [Range(0, 1)]
        [Slider]
        [DefaultValue(1f)]
        public float ScreenshakeMult = 1;

        [Label("Lighting Buffer Poll Rate")]
        [Tooltip("Changes how often the lighting buffer polls for data. \nHigher values increase performance but make lighting update slower on some objects. \nLower values result in smoother moving light but may hurt performance.")]
        [Range(1, 30)]
        [DrawTicks]
        [Slider]
        [DefaultValue(5f)]
        public int LightingPollRate = 5;

        [Label("Scrolling Lighting Buffer Building")]
        [Tooltip("Causes the lighting buffer to be built over its poll rate instead of all at once. \nMay help normalize lag spikes but cause strange lighting artifacts.")]
        [DefaultValue(false)]
        public bool ScrollingLightingPoll = false;

        [Label("Extra Particles")]
        [Tooltip("Enables/Disables special particles. \nDisable this if you have performance issues.")]
        [DefaultValue(true)]
        public bool ParticlesActive = true;

        [Label("High quality lit textures")]
        [Tooltip("Enables/Disables fancy lighting on large textures. \nDisable this if you have performance issues.")]
        [DefaultValue(true)]
        public bool HighQualityLighting = true;
    }
}