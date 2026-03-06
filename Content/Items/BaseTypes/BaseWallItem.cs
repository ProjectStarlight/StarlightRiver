using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class BaseWallItem : ModItem
	{
		public string itemName;
		public string itemToolTip;
		private readonly int wallType;
		private readonly int rarity;
		private readonly string texturePath;
		private readonly bool pathHasName;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? base.Texture : texturePath + (pathHasName ? string.Empty : Name);

		protected BaseWallItem(string name, string tooltip, int placetype, int rare, string texturePath = null, bool pathHasName = false)
		{
			itemName = name;
			itemToolTip = tooltip;
			wallType = placetype;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(itemName);
			Tooltip.SetDefault(itemToolTip);
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.createWall = wallType;
			Item.rare = rarity;
		}
	}
}