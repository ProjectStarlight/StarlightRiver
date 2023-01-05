using Terraria.ModLoader.Config;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.ComponentModel;

namespace StarlightRiver.Configs
{
    public class GameplayConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

		public override bool NeedsReload(ModConfig pendingConfig)
		{
			if (Lightsabers != (pendingConfig as GameplayConfig).Lightsabers) 
				return true;
			return false;
		}

		[Label("Custom Phasesabers")]
		[Tooltip("Whether or not phasesabers have custom behavior")]
		[DefaultValue(typeof(bool), "true")]
		public bool Lightsabers = true;
    }
}