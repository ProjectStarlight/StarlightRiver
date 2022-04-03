using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

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

			for(int k = 0; k < openBag.Items.Count; k++)
			{
				grid.Add(new IngredientStorageSlot(openBag.Items[k], k));
			}
		}
	}

	class IngredientStorageSlot : UIElement
	{
		public Item Item;

		public IngredientStorageSlot(Item Item, int index)
		{
			this.Item = Item;

			Width.Set(50, 0);
			Height.Set(50, 0);

			Left.Set(index % 8 * 54, 0);
			Top.Set(index / 8 * 54, 0);
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
			{
				Main.HoverItem = Item;
				//ItemSlot.mouse
			}
			
			base.Update(gameTime);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var tex = Request<Texture2D>(AssetDirectory.GUI + "FoodSlot").Value;
			var ItemTex = Main.PopupTexture[Item.type];
			var pos = GetDimensions().Center();

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(Item.rare), 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(ItemTex, pos, null, Color.White, 0, ItemTex.Size() / 2, 1, 0, 0);

			if(IsMouseHovering)
			{
				//Main.HoverItem = Item;
				//typeof(Main).GetMethod("MouseText_DrawItemTooltip", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Main.instance, new Object[] { Item.rare, (byte)0, Main.mouseX, Main.mouseY });
			}
		}
	}

}
