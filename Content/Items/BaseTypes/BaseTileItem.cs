using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class BaseTileItem : ModItem
	{
		public string internalName = "";
		public string itemName;
		public string itemToolTip;
		//private readonly int Tiletype;
		private readonly string tileName;
		private readonly int rarity;
		private readonly string texturePath;
		private readonly bool pathHasName;
		private readonly int itemValue;

		public override string Name => internalName != "" ? internalName : base.Name;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? AssetDirectory.Debug : texturePath + (pathHasName ? string.Empty : Name);

		public BaseTileItem() { }

		public BaseTileItem(string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
		{
			itemName = name;
			itemToolTip = tooltip;
			tileName = placetype;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
			itemValue = ItemValue;
		}

		public BaseTileItem(string internalName, string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
		{
			this.internalName = internalName;
			itemName = name;
			itemToolTip = tooltip;
			tileName = placetype;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
			itemValue = ItemValue;
		}

		public virtual void SafeSetDefaults() { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(itemName ?? "ERROR");
			Tooltip.SetDefault(itemToolTip ?? "Report me please!");
		}

		public override void SetDefaults()
		{
			if (tileName is null)
				return;

			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.createTile = Mod.Find<ModTile>(tileName).Type;
			Item.rare = rarity;
			Item.value = itemValue;
			SafeSetDefaults();
		}
	}
}
