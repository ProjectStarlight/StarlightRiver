using Terraria.ModLoader.Config;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.ComponentModel;

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

        [Label("Lighting buffer update delay")]
        [Tooltip("The delay between updating the lighting buffer")]
        [Range(2, 20)]
        [DrawTicks]
        [Slider]
        [DefaultValue(5f)]
        public int LightingUpdateDelay = 5;

        [Label("High quality lit textures")]
        [Tooltip("Enables/Disables fancy lighting on large textures. Disable this if you have performance issues.")]
        public bool HighQualityLighting = true;

        [Label("Custom Inventory Sounds")]
        [Tooltip("If custom inventory sounds should play for all items or a select few, or none at all.")]
        public CustomSounds InvSounds = CustomSounds.All;
    }
}