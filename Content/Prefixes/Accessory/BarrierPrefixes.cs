using StarlightRiver.Core.Systems.BarrierSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory
{
	internal abstract class BarrierPrefix : ModPrefix
	{
		private readonly int barrier;
		private readonly string name;
		private readonly string tip;

		internal BarrierPrefix(int barrier, string name, string tip)
		{
			this.barrier = barrier;
			this.name = name;
			this.tip = tip;
		}

		public override bool CanRoll(Item item)
		{
			return item.accessory;
		}

		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(name);
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 1 + barrier / 100;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.GetModPlayer<BarrierPlayer>().maxBarrier += barrier;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "BarrierTip", tip)
			{
				IsModifier = true
			};
		}
	}

	internal class LayeredPrefix : BarrierPrefix
	{
		public LayeredPrefix() : base(4, "Layered", "+4 maximum {{Barrier}}") { }
	}

	internal class DefensivePrefix : BarrierPrefix
	{
		public DefensivePrefix() : base(8, "Defensive", "+8 maximum {{Barrier}}") { }
	}

	internal class PlatedPrefix : BarrierPrefix
	{
		public PlatedPrefix() : base(12, "Plated", "+12 maximum {{Barrier}}") { }
	}

	internal class ReinforcedPrefix : BarrierPrefix
	{
		public ReinforcedPrefix() : base(16, "Reinforced", "+16 maximum {{Barrier}}") { }
	}
}