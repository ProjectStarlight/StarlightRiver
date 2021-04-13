using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using StarlightRiver.Content.Items.Utility;
using Terraria.ModLoader.UI.Elements;
using Microsoft.Xna.Framework.Graphics;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.Food;
using StarlightRiver.Helpers;
using System.Reflection;

namespace StarlightRiver.Content.GUI
{
	class ChefBagUI : SmartUIState
	{
		public static ChefBag openBag = null;

		public UIGrid grid = new UIGrid();

		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 1;
		}

		public override void OnInitialize()
		{
			Elements.Clear();
			grid.Clear();

			grid.Left.Set(-220, 0.5f);
			grid.Top.Set(-220, 0.5f);
			grid.Width.Set(440, 0);
			grid.Height.Set(440, 0);
			grid.ListPadding = 4;
			grid.MaxWidth.Set(440, 0);
			grid.MaxHeight.Set(440, 0);

			Append(grid);

			if (openBag is null)
				return;

			for(int k = 0; k < openBag.items.Count; k++)
			{
				grid.Add(new IngredientStorageSlot(openBag.items[k], k));
			}
		}
	}

	class IngredientStorageSlot : UIElement
	{
		public Item item;

		public IngredientStorageSlot(Item item, int index)
		{
			this.item = item;

			Width.Set(50, 0);
			Height.Set(50, 0);

			Left.Set(index % 8 * 54, 0);
			Top.Set(index / 8 * 54, 0);
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
			{
				Main.HoverItem = item;
				//ItemSlot.mouse
			}
			
			base.Update(gameTime);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var tex = GetTexture(AssetDirectory.GUI + "FoodSlot");
			var itemTex = Main.itemTexture[item.type];
			var pos = GetDimensions().Center();

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(item.rare), 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(itemTex, pos, null, Color.White, 0, itemTex.Size() / 2, 1, 0, 0);

			if(IsMouseHovering)
			{
				//Main.HoverItem = item;
				//typeof(Main).GetMethod("MouseText_DrawItemTooltip", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Main.instance, new Object[] { item.rare, (byte)0, Main.mouseX, Main.mouseY });
			}
		}
	}

}
