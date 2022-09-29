﻿using Terraria.ModLoader.Config;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.ComponentModel;

namespace StarlightRiver.Configs
{
    public enum CustomSounds
    {
        All = 0,
        Specific = 1,
        None = 2
    }

    public class AudioConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Custom Inventory Sounds")]
        [DrawTicks]
        [Tooltip("If custom inventory sounds should play for all Items, select few, or none.")]
        [DefaultValue(typeof(CustomSounds), "All")]
        public CustomSounds InvSounds = CustomSounds.All;
    }
}