using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Buffs
{
	public class CrimsonHallucination : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "CrimsonHallucination";

		public CrimsonHallucination() : base("Hallucinating", "Is any of this real?", true) { }
	}
}