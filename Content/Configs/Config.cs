using Terraria.ModLoader.Config;

namespace StarlightRiver.Configs
{
	public enum TitleScreenStyle
    {
        Starlight = 0,
        Vitric = 1,
        Overgrow = 2,
        CorruptJungle = 3,
        CrimsonJungle = 4,
        HallowJungle = 5,
        None = 6
    }

    public enum LightImportance //feel free to rename
    {
        All = 0,
        Most = 1,
        Some = 2,
        Minimal = 3
    }

    public enum CustomSounds
    {
        All = 0,
        Specific = 1,
        None = 2

    }

    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Menu Theme")]
        [Tooltip("Changes or disables the menu theme")]
        public TitleScreenStyle Style;

        [Label("Lighting Buffer Poll Rate")]
        [Tooltip("Changes how often the lighting buffer polls for data. Higher values increase performance but make lighting update slower on some objects. Lower values result in smoother moving light but may hurt performance.")]
        [Range(1, 30)]
        public int LightingPollRate = 5;

        [Label("Scrolling Lighting Buffer Building")]
        [Tooltip("Causes the lighting buffer to be built over its poll rate instead of all at once. May help normalize lag spikes but cause strange lighting artifacts.")]
        public bool ScrollingLightingPoll = false;

        [Label("Extra Particles")]
        [Tooltip("Enables/Disables special particles. Disable this if you have performance issues.")]
        public bool ParticlesActive = true;

        [Label("Texture Lighting")]
        [Tooltip("Enables/Disables lighting on large textures particles. Disable this if you have performance issues.")]
        public LightImportance TextureLighting;

        [Label("Custom Inventory Sounds")]
        [Tooltip("If custom inventory sounds should play for all items or a select few, or none at all.")]
        public CustomSounds InvSounds = CustomSounds.All;
    }
}