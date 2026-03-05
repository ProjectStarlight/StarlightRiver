using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class BaseCritterItem : ModItem
	{
		private readonly string itemName;
		private readonly string itemTooltip;
		private readonly int maxStack;
		private readonly int value;
		private readonly int rarity;
		private readonly int npcID;
		private readonly string texturePath;
		private readonly bool pathHasName;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? base.Texture : texturePath + (pathHasName ? string.Empty : Name);

		protected BaseCritterItem(string name, string tooltip, int value, int rare, int NPCType, string texturePath = null, bool pathHasName = false, int maxstack = 999)
		{
			itemName = name;
			itemTooltip = tooltip;
			maxStack = maxstack;
			this.value = value;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
			npcID = NPCType;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(itemName);
			Tooltip.SetDefault(itemTooltip);
		}

		public override void SetDefaults()
		{
			Item.consumable = true;

			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useTime = Item.useAnimation = 15;
			Item.makeNPC = npcID;

			Item.width = 20;
			Item.height = 20;
			Item.maxStack = maxStack;
			Item.value = value;
			Item.rare = rarity;
		}
	}
}
