using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.BaseTypes;

public abstract class BaseMaterial : ModItem
{
	private readonly string name;
	private readonly string tooltip;
	private readonly int maxStack;
	private readonly int value;
	private readonly int rarity;
	private readonly string texturePath;
	private readonly bool pathHasName;

	public override string Texture => string.IsNullOrEmpty(texturePath) ? base.Texture : texturePath + (pathHasName ? string.Empty : Name);

	protected BaseMaterial(string name, string tooltip, int maxstack, int value, int rare, string texturePath = null, bool pathHasName = false)
	{
		this.name = name;
		this.tooltip = tooltip;
		maxStack = maxstack;
		this.value = value;
		rarity = rare;
		this.texturePath = texturePath;
		this.pathHasName = pathHasName;
	}

	public override void SetStaticDefaults()
	{
		DisplayName.SetDefault(name);
		Tooltip.SetDefault(tooltip);
	}

	public override void SetDefaults()
	{
		Item.width = 16;
		Item.height = 16;
		Item.maxStack = maxStack;
		Item.value = value;
		Item.rare = rarity;
	}
}