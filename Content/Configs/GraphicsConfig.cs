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

        [Label("Background Reflections")]
        [Tooltip("Configures what is rendered for background reflections. \nDisable this if you have performance issues.")]
        public ReflectionSubConfig ReflectionConfig = new ReflectionSubConfig();
    }

    public class ReflectionSubConfig
    {
        [Label("Background Reflections")]
        [Tooltip("This will Enable/Disable the background reflections system entirely. \nIf this is off, the other settings in this block are ignored. \nDisable this if you have performance issues.")]
        [DefaultValue(true)]
        public bool ReflectionsOn = true;

        [Label("Reflect Players")]
        [Tooltip("This will Enable/Disable reflecting players.\n low performance impact.")]
        [DefaultValue(true)]
        public bool PlayerReflectionsOn = true;

        [Label("Reflect NPCs")]
        [Tooltip("This will Enable/Disable reflecting NPCs.\n low performance impact.")]
        [DefaultValue(true)]
        public bool NpcReflectionsOn = true;

        [Label("Reflect Projectiles")]
        [Tooltip("This will Enable/Disable reflecting Projectiles.\n high performance impact.")]
        [DefaultValue(true)]
        public bool ProjReflectionsOn = true;

        [Label("Reflect Particles")]
        [Tooltip("This will Enable/Disable reflecting Particles, gores and dusts.\n high performance impact.")]
        [DefaultValue(true)]
        public bool DustReflectionsOn = true;

        public override bool Equals(object obj)
        {
            if (obj is ReflectionSubConfig other)
            {
                return other.ReflectionsOn == this.ReflectionsOn
                        && other.PlayerReflectionsOn == this.PlayerReflectionsOn
                        && other.NpcReflectionsOn == this.NpcReflectionsOn
                        && other.ProjReflectionsOn == this.ProjReflectionsOn
                        && other.DustReflectionsOn == this.DustReflectionsOn;
            }

            return base.Equals(obj);
        }


        /// <summary>
        /// Incase someone disables all the individual reflection components without disabling the entire system, we still want the extra optimization of disabling the entire system
        /// so this checks each indiviudal reflection component and returns true if any are reflecting while the system is on
        /// </summary>
        /// <returns></returns>
        public bool isReflectingAnything()
        {
            return ReflectionsOn && (PlayerReflectionsOn || NpcReflectionsOn || ProjReflectionsOn || DustReflectionsOn);
        }

        public override int GetHashCode()
        {
            return new { ReflectionsOn, PlayerReflectionsOn, NpcReflectionsOn, ProjReflectionsOn, DustReflectionsOn }.GetHashCode();
        }
    }
}