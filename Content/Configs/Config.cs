using Terraria.ModLoader.Config;

using StarlightRiver.Core;

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

    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Menu Theme")]
        [Tooltip("Changes or disables the menu theme")]
        public TitleScreenStyle Style;

        [Label("Extra Particles")]
        [Tooltip("Enables/Disables special particles. Disable this if you have performance issues.")]
        public bool Active = true;
    }
}