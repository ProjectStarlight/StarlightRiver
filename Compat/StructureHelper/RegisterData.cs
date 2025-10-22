using StarlightRiver.Core.Systems.AuroraWaterSystem;
using StructureHelper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Compat.StructureHelper
{
	internal class RegisterData : ModSystem
	{
		public override void OnModLoad()
		{
			if (ModLoader.TryGetMod("StructureHelper", out Mod sh))
			{
				sh.Call("RegisterCustomData", typeof(AuroraWaterData), Mod);
			}
		}
	}
}
