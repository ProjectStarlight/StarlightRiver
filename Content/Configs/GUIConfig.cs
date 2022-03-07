using Terraria.ModLoader.Config;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.ComponentModel;

namespace StarlightRiver.Configs
{
    public enum OverlayState
    {
        AlwaysOn = 0,
        WhileNotFull = 1,
        WhileUsing = 2,
        Never = 3
    }

    public class GUIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Overhead Stamina Display")]
        [DrawTicks]
        [Tooltip("When/If the overhead stamina meter should display")]
        [DefaultValue(typeof(OverlayState), "WhileNotFull")]
        public OverlayState OverheadStaminaState = OverlayState.WhileNotFull;
    }
}