using StarlightRiver.Core.Systems.InoculationSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory;

internal abstract class DoTResistPrefix : ModPrefix
{
	private readonly float power;
	private readonly string name;
	private readonly string tip;

	internal DoTResistPrefix(float power, string name, string tip)
	{
		this.power = power;
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
		valueMult *= 1 + 0.05f * power;
	}

	public override void ApplyAccessoryEffects(Player player)
	{
		player.GetModPlayer<InoculationPlayer>().DoTResist += power;
	}

	public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		yield return new TooltipLine(StarlightRiver.Instance, "DotResistTip", tip)
		{
			IsModifier = true
		};
	}
}

internal class DoTResistPrefix1 : DoTResistPrefix
{
	public DoTResistPrefix1() : base(0.02f, "Healthy", "+2% {{Inoculation}}") { }
}

internal class DoTResistPrefix2 : DoTResistPrefix
{
	public DoTResistPrefix2() : base(0.04f, "Protected", "+4% {{Inoculation}}") { }
}

internal class DoTResistPrefix3 : DoTResistPrefix
{
	public DoTResistPrefix3() : base(0.05f, "Blessed", "+5% {{Inoculation}}") { }
}