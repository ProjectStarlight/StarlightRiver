using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.CombatMountPrefixes
{
	public abstract class CombatMountPrefix : CustomTooltipPrefix
	{
		public static List<int> combatMountPrefixTypes = new();

		/// <summary>
		/// Modify the mount's stats here. The other apply methods wont do anything to the mount itself.
		/// </summary>
		/// <param name="mount"></param>
		public virtual void ApplyToMount(CombatMount mount)
		{

		}

		public override void SetStaticDefaults()
		{
			if (Type != 0)
				combatMountPrefixTypes.Add(Type);
		}

		public sealed override void Unload()
		{
			combatMountPrefixTypes = null;
		}

		public sealed override bool CanRoll(Item item) //Only combat mounts should get these.
		{
			return item.ModItem is CombatMountItem;
		}
	}
}