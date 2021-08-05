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