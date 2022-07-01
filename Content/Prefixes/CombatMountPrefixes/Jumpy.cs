using StarlightRiver.Content.Abilities;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core.Systems.CombatMountSystem;

namespace StarlightRiver.Prefixes.CombatMountPrefixes
{
	public class Jumpy : CombatMountPrefix
	{
		public Jumpy() : base("+15% Attack Speed\n+10% Movement Speed") { }

		public override void ApplyToMount(CombatMount mount)
		{
			mount.primarySpeedMultiplier -= 0.5f;
			mount.moveSpeedMultiplier += 0.1f;
		}
	}
}
